
document.addEventListener('DOMContentLoaded', function () {
  const sidebar = document.getElementById('appSidebar');
    if (!sidebar) return;

  // Ensure nothing is accidentally expanded at start
  sidebar.querySelectorAll('.nav-group-items').forEach(el => {
        el.classList.remove('show');
  });
  sidebar.querySelectorAll('.nav-link.nav-group-toggle').forEach(t => {
        t.setAttribute('aria-expanded', 'false');
  });

    // Delegate clicks - only one event listener on the sidebar
    sidebar.addEventListener('click', function (e) {
    const toggle = e.target.closest('.nav-link.nav-group-toggle');
    if (!toggle || !sidebar.contains(toggle)) return;

    e.preventDefault();

    // parent nav-group <li>
        const parentLi = toggle.closest('.nav-group');
        if (!parentLi) return;

        // find direct child submenu (only immediate child with class .nav-group-items)
        let submenu = null;
        for (let i = 0; i < parentLi.children.length; i++) {
      const c = parentLi.children[i];
        if (c.classList && c.classList.contains('nav-group-items')) {submenu = c; break; }
    }
        if (!submenu) return;

        const isOpen = submenu.classList.contains('show');

        // Close sibling submenus at the same level
        const parentUl = parentLi.parentElement;
        if (parentUl) {
            Array.from(parentUl.children).forEach(siblingLi => {
                if (siblingLi === parentLi) return;
                const sibSub = Array.from(siblingLi.children).find(c => c.classList && c.classList.contains('nav-group-items'));
                if (sibSub && sibSub.classList.contains('show')) {
                    // close sibling
                    sibSub.classList.remove('show');
                    // update its toggle aria-expanded if found
                    const sibToggle = siblingLi.querySelector('.nav-link.nav-group-toggle');
                    if (sibToggle) sibToggle.setAttribute('aria-expanded', 'false');
                }
            });
    }

        // Toggle current submenu
        if (isOpen) {
            submenu.classList.remove('show');
        toggle.setAttribute('aria-expanded', 'false');
    } else {
            submenu.classList.add('show');
        toggle.setAttribute('aria-expanded', 'true');
    }
  }, false);
});