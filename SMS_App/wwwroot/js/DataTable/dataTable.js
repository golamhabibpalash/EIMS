// Search / paging / sorting for the standard list tables.
//
// This used to be written as $(function (selector) { $(selector).DataTable(...) }).
// That is the document-ready shorthand, so "selector" was jQuery itself and the
// call matched nothing - every page including this file got no DataTable at all.
// Every list table in the app uses id="myTable", so bind to that.
$(function () {
    var $table = $('#myTable');

    if ($table.length === 0 || !$.fn.DataTable) {
        return;
    }
    if ($.fn.DataTable.isDataTable($table)) {
        return;
    }
    // A table with no <th> in its thead, or a colspan filler row in its tbody, makes
    // DataTables throw. Skip those rather than letting the error escape and take the
    // rest of the page's scripts with it.
    if ($table.find('thead th').length === 0) {
        return;
    }

    try {
        $table.DataTable({
            // Only the DataTables core is loaded, so no Buttons ("B") or Responsive here.
            dom: '<"row mb-2"<"col-sm-6"l><"col-sm-6"f>>rt<"row mt-2"<"col-sm-5"i><"col-sm-7"p>>',
            pagingType: 'full_numbers',
            lengthMenu: [[30, 50, 100, -1], [30, 50, 100, 'All']],
            pageLength: 30,
            autoWidth: false,
            stateSave: true,
            // Every table shares the id "myTable", so key saved state by page as well -
            // otherwise a search typed on one list is restored on all the others.
            stateSaveCallback: function (settings, data) {
                try {
                    localStorage.setItem('DataTables_' + settings.sInstance + '_' + location.pathname, JSON.stringify(data));
                } catch (e) { /* storage unavailable - state just is not remembered */ }
            },
            // Per-column searches are driven by page-level filter dropdowns. Restoring
            // them would leave the grid filtered while every dropdown still reads
            // "All", which looks exactly like broken filtering. Keep page length,
            // ordering and the global search box; drop the column searches.
            stateLoadParams: function (settings, data) {
                if (data && data.columns) {
                    data.columns.forEach(function (col) {
                        if (col && col.search) { col.search.search = ''; }
                    });
                }
            },
            stateLoadCallback: function (settings) {
                try {
                    return JSON.parse(localStorage.getItem('DataTables_' + settings.sInstance + '_' + location.pathname));
                } catch (e) { return null; }
            },
            columnDefs: [
                { targets: 'no-sort', orderable: false },
                { targets: 'no-search', searchable: false }
            ]
        });
    } catch (e) {
        console.warn('DataTable not applied to #myTable:', e.message);
    }
});
