#!/bin/bash
# ============================================================
# EIMS Multi-Instance Deployment Script
# Run this on your VPS after the first-time setup
# Usage: sudo ./deploy.sh
# ============================================================

set -euo pipefail

# ---- CONFIGURATION (EDIT THESE) ----
DOMAIN="yourdomain.com"
APP_DIR="/var/www/eims"
RELEASE_DIR="$APP_DIR/release"
DOTNET_VERSION="8.0"
NGINX_AVAILABLE="/etc/nginx/sites-available"
NGINX_ENABLED="/etc/nginx/sites-enabled"

# List your school instances: "subdomain:port:dbname"
INSTANCES=(
    "school1:5001:eimsdb_school1"
    "school2:5002:eimsdb_school2"
    "school3:5003:eimsdb_school3"
)

# PostgreSQL credentials
PG_USER="ghp"
PG_PASSWORD="12345AsD,./"

# ---- COLOR OUTPUT ----
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

info()  { echo -e "${GREEN}[INFO]${NC} $1"; }
warn()  { echo -e "${YELLOW}[WARN]${NC} $1"; }
error() { echo -e "${RED}[ERROR]${NC} $1"; }

# ---- PREREQUISITES CHECK ----
check_prereqs() {
    info "Checking prerequisites..."

    if ! command -v dotnet &>/dev/null; then
        error ".NET SDK not found. Install .NET $DOTNET_VERSION first."
        exit 1
    fi

    if ! command -v nginx &>/dev/null; then
        error "Nginx not found. Install it first: sudo apt install nginx"
        exit 1
    fi

    if ! command -v psql &>/dev/null; then
        warn "psql not found locally. Make sure PostgreSQL is running."
    fi

    info "All prerequisites met."
}

# ---- BUILD & PUBLISH ----
build_app() {
    info "Building and publishing EIMS..."
    rm -rf "$RELEASE_DIR"
    dotnet publish "$APP_DIR/SMS_App/SMS_App.csproj" \
        --configuration Release \
        --output "$RELEASE_DIR" \
        --runtime linux-x64 \
        --self-contained false
    info "Build complete."
}

# ---- CREATE INSTANCE DIRECTORIES ----
setup_instances() {
    info "Setting up instance directories..."

    for instance in "${INSTANCES[@]}"; do
        IFS=":" read -r subdomain port dbname <<< "$instance"
        local inst_dir="$APP_DIR/$subdomain"

        mkdir -p "$inst_dir"
        cp -r "$RELEASE_DIR"/* "$inst_dir/"

        # Create appsettings.json with PostgreSQL connection
        cat > "$inst_dir/appsettings.json" << JSON
{
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=${dbname};Username=${PG_USER};Password=${PG_PASSWORD}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "PhoneSMSSetup": {
    "PhoneSMSVendorAPILink": "http://api.greenweb.com.bd/api.php?",
    "Token": ""
  },
  "Hangfire": {
    "DashboardPath": "/hangfire",
    "WorkerCount": 2,
    "IsEnabled": false
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://127.0.0.1:${port}"
      }
    }
  }
}
JSON

        info "  Created $subdomain (port $port, DB: $dbname)"
    done
}

# ---- CREATE POSTGRESQL DATABASES ----
create_databases() {
    info "Creating PostgreSQL databases..."

    for instance in "${INSTANCES[@]}"; do
        IFS=":" read -r subdomain port dbname <<< "$instance"

        # Create database (ignore error if exists)
        docker exec eims-postgres psql -U "$PG_USER" -tc \
            "SELECT 1 FROM pg_database WHERE datname = '$dbname'" | grep -q 1 \
            && info "  Database $dbname already exists" \
            || {
                docker exec eims-postgres psql -U "$PG_USER" -c "CREATE DATABASE $dbname;"
                info "  Created database: $dbname"
            }
    done
}

# ---- APPLY DATABASE SCHEMA (generate & run SQL for each) ----
setup_schemas() {
    info "Generating and applying database schemas..."

    local script_dir="/tmp/eims-sql"
    mkdir -p "$script_dir"

    for instance in "${INSTANCES[@]}"; do
        IFS=":" read -r subdomain port dbname <<< "$instance"

        # Generate SQL from the EF Core model, targeting the specific database
        local sql_file="$script_dir/$dbname.sql"
        export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=${dbname};Username=${PG_USER};Password=${PG_PASSWORD}"

        # Run schema script (capture the EF model into SQL)
        cd "$APP_DIR/$subdomain"
        dotnet ef dbcontext script --output "$sql_file" 2>/dev/null || {
            warn "  EF Core script generation failed. Direct schema setup not possible this way."
            info "  You can apply schema using a shared migration or manually."
        }

        if [ -f "$sql_file" ]; then
            docker exec -i eims-postgres psql -U "$PG_USER" -d "$dbname" < "$sql_file"
            info "  Schema applied to $dbname"
        fi

        unset ConnectionStrings__DefaultConnection
    done

    rm -rf "$script_dir"
}

# ---- CREATE SYSTEMD SERVICE FILES ----
create_services() {
    info "Creating systemd service files..."

    for instance in "${INSTANCES[@]}"; do
        IFS=":" read -r subdomain port dbname <<< "$instance"
        local svc_file="/etc/systemd/system/eims-${subdomain}.service"

        sudo tee "$svc_file" > /dev/null << SYSTEMD
[Unit]
Description=EIMS - $subdomain ($dbname)
After=network.target postgresql.service
Wants=postgresql.service

[Service]
Type=simple
WorkingDirectory=$APP_DIR/$subdomain
ExecStart=/usr/bin/dotnet $APP_DIR/$subdomain/SMS_App.dll --urls "http://127.0.0.1:${port}"
Restart=always
RestartSec=10
User=www-data
Group=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_CLI_TELEMETRY_OPTOUT=1

[Install]
WantedBy=multi-user.target
SYSTEMD

        info "  Created service: eims-${subdomain}.service"
    done

    sudo systemctl daemon-reload
}

# ---- START ALL INSTANCES ----
start_instances() {
    info "Starting all instances..."

    for instance in "${INSTANCES[@]}"; do
        IFS=":" read -r subdomain port dbname <<< "$instance"
        sudo systemctl enable "eims-${subdomain}"
        sudo systemctl start "eims-${subdomain}"
        info "  Started eims-${subdomain}"
    done
}

# ---- SETUP NGINX ----
setup_nginx() {
    info "Configuring Nginx..."

    local nginx_conf="$NGINX_AVAILABLE/eims"
    sudo tee "$nginx_conf" > /dev/null << NGINX
# EIMS Multi-Instance Nginx Configuration
# Generated by deploy.sh on $(date)

NGINX

    for instance in "${INSTANCES[@]}"; do
        IFS=":" read -r subdomain port dbname <<< "$instance"
        sudo tee -a "$nginx_conf" > /dev/null << NGINX

server {
    listen 80;
    server_name ${subdomain}.${DOMAIN};

    location / {
        proxy_pass http://127.0.0.1:${port};
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
NGINX
        info "  Added Nginx block: ${subdomain}.${DOMAIN}"
    done

    if [ -f "$NGINX_ENABLED/eims" ]; then
        sudo rm "$NGINX_ENABLED/eims"
    fi
    sudo ln -sf "$nginx_conf" "$NGINX_ENABLED/eims"
    sudo nginx -t && sudo systemctl reload nginx
    info "Nginx configuration applied."
}

# ---- INSTALL SSL WITH CERTBOT ----
setup_ssl() {
    info "Setting up SSL with Certbot..."

    local domains=""
    for instance in "${INSTANCES[@]}"; do
        IFS=":" read -r subdomain port dbname <<< "$instance"
        if [ -z "$domains" ]; then
            domains="-d ${subdomain}.${DOMAIN}"
        else
            domains="$domains -d ${subdomain}.${DOMAIN}"
        fi
    done

    sudo certbot --nginx $domains --non-interactive --agree-tos -m admin@${DOMAIN} || {
        warn "SSL setup failed or certbot not installed."
        info "  Run later: sudo certbot --nginx $domains"
    }
}

# ---- STATUS CHECK ----
show_status() {
    echo ""
    info "========================================"
    info "Deployment Complete!"
    info "========================================"
    echo ""

    for instance in "${INSTANCES[@]}"; do
        IFS=":" read -r subdomain port dbname <<< "$instance"
        local status=$(sudo systemctl is-active "eims-${subdomain}" 2>/dev/null || echo "inactive")
        echo "  ${subdomain}.${DOMAIN}  →  port ${port}  →  DB: ${dbname}  [${status}]"
    done

    echo ""
    info "Nginx sites enabled:"
    sudo nginx -T 2>/dev/null | grep "server_name" | head -10

    echo ""
    info "Instances running:"
    sudo systemctl list-units --type=service --state=running | grep eims- || echo "  (none running yet)"
}

# ============================================================
# MAIN
# ============================================================

case "${1:-all}" in
    check)     check_prereqs ;;
    build)     build_app ;;
    instances) setup_instances ;;
    db)        create_databases ;;
    schema)    setup_schemas ;;
    services)  create_services ;;
    start)     start_instances ;;
    nginx)     setup_nginx ;;
    ssl)       setup_ssl ;;
    status)    show_status ;;
    all)
        check_prereqs
        build_app
        setup_instances
        create_databases
        setup_schemas
        create_services
        start_instances
        setup_nginx
        setup_ssl
        show_status
        ;;
    *)
        echo "Usage: $0 {check|build|instances|db|schema|services|start|nginx|ssl|status|all}"
        echo ""
        echo "Steps (run in order):"
        echo "  $0 check       - Verify prerequisites"
        echo "  $0 build       - Build & publish the app"
        echo "  $0 instances   - Create instance directories with configs"
        echo "  $0 db          - Create PostgreSQL databases"
        echo "  $0 schema      - Apply database schemas"
        echo "  $0 services    - Create systemd service files"
        echo "  $0 start       - Start all instances"
        echo "  $0 nginx       - Configure Nginx reverse proxy"
        echo "  $0 ssl         - Install SSL certificates"
        echo "  $0 status      - Show deployment status"
        echo ""
        echo "Or just run: $0 all (does everything above)"
        ;;
esac
