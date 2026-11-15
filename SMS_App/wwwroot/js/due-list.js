/*
 * Shared behaviour for the due lists (Views/StudentPayments/_DueListResults.cshtml):
 * sortable/searchable DataTables grid, serial numbers and totals that follow the search,
 * Export (server-side, same rows and order as on screen) and Print.
 *
 *   DueList.init({ exportUrl: '/StudentPayments/DuePaymentExport', applied: { ...filters the list was loaded with } });
 */
window.DueList = (function ($) {
    'use strict';

    const amountFormat = new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 });

    function colIndex($table, cls) {
        return $table.find('thead th').index($table.find('thead th.' + cls));
    }

    function init(options) {
        const $table = $('#dueListTable');
        if (!$table.length || !$.fn.DataTable) return null;

        const cols = {
            payable: colIndex($table, 'sum-payable'),
            paid: colIndex($table, 'sum-paid'),
            due: colIndex($table, 'sum-due'),
            defaultSort: colIndex($table, 'default-sort')
        };
        const textCols = $table.find('thead th').map(function (i) { return $(this).hasClass('sort-text') ? i : null; }).get();

        const table = $table.DataTable({
            order: [[cols.defaultSort, 'desc']],
            pageLength: 50,
            lengthMenu: [[25, 50, 100, -1], [25, 50, 100, 'All']],
            columnDefs: [
                { targets: 'no-sort', orderable: false, searchable: false },
                // IDs and phone numbers sort as text, the same way the export sorts them.
                { targets: textCols, type: 'string' }
            ],
            language: {
                search: '',
                searchPlaceholder: 'Search name, roll, ID, phone…',
                emptyTable: 'No dues found',
                zeroRecords: 'No students match your search'
            },
            drawCallback: function () {
                const api = this.api();
                // Serial numbers follow the current sort and search, not the original order.
                api.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) { cell.textContent = i + 1; });

                // Totals (summary cards and footer) follow the quick search.
                const rows = api.rows({ search: 'applied' });
                const sum = function (idx) {
                    return api.cells(rows.indexes(), idx).nodes().toArray()
                        .reduce(function (a, td) { return a + (parseFloat(td.getAttribute('data-order')) || 0); }, 0);
                };
                $('#sumCount, #footCount').text(rows.count());
                $('#sumPayable, #footPayable').text(amountFormat.format(sum(cols.payable)));
                $('#sumPaid, #footPaid').text(amountFormat.format(sum(cols.paid)));
                $('#sumDue, #footDue').text(amountFormat.format(sum(cols.due)));
            }
        });

        $('.export-link').on('click', function (e) {
            e.preventDefault();
            const p = new URLSearchParams();
            $.each(options.applied || {}, function (k, v) { if (v !== null && v !== undefined && v !== '') p.set(k, v); });
            const order = table.order()[0];
            if (order) {
                const key = $(table.column(order[0]).header()).data('sort-key');
                if (key) { p.set('sortBy', key); p.set('sortDir', order[1]); }
            }
            if (table.search()) p.set('search', table.search());
            p.set('format', $(this).data('format'));
            window.location.href = options.exportUrl + '?' + p.toString();
        });

        $('#printBtn').on('click', function () {
            // Print every matching row, not just the current page, then restore paging.
            const length = table.page.len();
            table.page.len(-1).draw(false);
            window.print();
            table.page.len(length).draw(false);
        });

        return table;
    }

    /** Spinner + disabled state on the "Get list" button while the form posts. */
    function busyOnSubmit(formSelector, buttonSelector) {
        $(formSelector).on('submit', function (e) {
            if (e.isDefaultPrevented()) return; // an earlier handler (e.g. validation) blocked the submit
            const $btn = $(buttonSelector).prop('disabled', true);
            $btn.find('.spinner-border').removeClass('d-none');
            $btn.find('.btn-label').text(' Loading…');
        });
    }

    return { init: init, busyOnSubmit: busyOnSubmit };
})(jQuery);
