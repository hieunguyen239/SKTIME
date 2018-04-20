jQuery.fn.tagName = function () {
    return this.prop("tagName").toLowerCase();
};
var _TableFunctions = {
    Parent: null,
    DefaultImage: null,
    ImageFolder: '',
    DateFormat: /^\/Date\(*/,
    Data: [],
    Input: [],
    OnRenderData: null,
    Init: function (info) {
        var e = info.e;
        if (!e)
            e = $('body');
        if (info.input) {
            _TableFunctions.Input.push(info.input);
        }
        $(e).find('table[data]').each(function (i, e) {
            _TableFunctions.RenderTableBody({
                table: e,
                size: 10
            });
        });
        $(e).find('.table-export[data]').on('click', function () {
            _TableFunctions.ExportExcel($('table[data="' + $(this).attr('data') + '"]'));
        });
        $(e).find('select[value]').each(function (i, e) {
            var name = $(this).attr('for');
            $('[name="' + $(this).attr('value') + '"]').on('change', function () {
                var vals = $(this).val().split(','),
                    names = [],
                    htmls = [],
                    input = $('[name="' + name + '"]');
                vals.forEach(function (e, i) {
                    var html = $('select[for="' + name + '"] option[value="' + e + '"]').html();
                    names.push(html);
                    htmls.push('<span class="it"><i id="cls">x</i><span>' + html + '</span></span>');
                });
                input.each(function (i, e) {
                    var tagName = $(e).tagName().toLowerCase();
                    switch (tagName) {
                        case 'input':
                            input.val(names.join(','));
                            break;
                        default:
                            input.html(htmls.join(''));
                            input.find('.it').on('click', function () {
                                $(this).toggleClass('act');
                            });
                            input.find('.it #cls').on('click', function () {
                                _TableFunctions.RemoveListItem($(this).parent());
                            });
                            break;
                    }
                });
            });
            $(e).on('change', _TableFunctions.SelectList);
        });
        $(e).find('.its[name] .it').each(function (i, e) {
            $(e).find('#cls').on('click', function () {
                _TableFunctions.RemoveListItem($(this).parent());
            });
            $(e).on('click', function () {
                $(this).toggleClass('act');
            });
        });
        $(window).on('keydown', function (e) {
            var keyCode = e.which || e.keyCode,
                key = e.key,
                remove = false;
            if (keyCode != 229) {
                if ($.inArray(keyCode, [46]) !== -1) {
                    remove = true;
                }
            }
            else if (key) {
                if ($.inArray(key, ["Delete"]) != -1) {
                    remove = true;
                }
            }
            if (remove) {
                $('.its[name] .it.act').each(function (i, e) {
                    _TableFunctions.RemoveListItem(e);
                });
            }
        });
    },
    RemoveListItem: function (it) {
        var select = $('select[for="' + $(it).parent().attr('name') + '"]'),
            valueInput = $('input[name="' + select.attr('value') + '"]'),
            nameInput = $('input[name="' + select.attr('for') + '"]'),
            name = $(it).find('span').html().trim(),
            value = select.find('option').filter(function (i, val) {
                return $(val).html().trim() == name;
            }),
            vals = valueInput.val().split(',').filter(_TableFunctions.RemoveNullOrEmpty);
        if (value.length > 0) {
            vals.splice(vals.indexOf($(value[0]).attr('value')), 1);
            valueInput.val(vals.join(','));
            names = nameInput.val().split(',').filter(_TableFunctions.RemoveNullOrEmpty);
            names.splice(names.indexOf(name), 1);
            nameInput.val(names.join(','));
            $(it).remove();
        }
    },
    RemoveNullOrEmpty: function (val) {
        return val != undefined && val != null && val != '';
    },
    SelectList: function () {
        var valInput = $('[name="' + $(this).attr('value') + '"]'),
            value = $(this).val(),
            name = $(this).find('option[value="' + value + '"]').html(),
            valInputValue = valInput.val(),
            vals = valInputValue ? valInputValue.split(',') : [];
        if (value != undefined && value != '' && vals.indexOf(value) == -1) {
            vals.push(value);
            vals = vals.filter(_TableFunctions.RemoveNullOrEmpty);
            valInput.val(vals.join(','));
            valInput.trigger('change');
            //val = $('input[name="Matches"]').val();
            //$('input[name="Matches"]').val(val + (val != '' ? ', ' : '') + name);
        }
    },
    ExportExcel: function (table) {
        var data = _TableFunctions.GetTableData(table);
        if (data != null && data.length > 0) {
            var tab_text = '<table border="1px"><tr style="color: #fff;">',
                textRange; var j = 0,
                iframe = document.createElement('iframe'),
                keys = $(table).attr('exkeys');

            if (keys != undefined && keys != '') {
                keys = keys.split(',');
            }
            else {
                keys = Object.keys(data[0]);
            }
            for (var i = 0; i < keys.length; i++) {
                tab_text += '<td width="200" style="background-color: rgb(91, 155, 213)"><strong>' + keys[i] + '</strong></td>';
            }
            tab_text += '</tr><tr>';
            for (var i = 0; i < data.length; i++) {
                var row = data[i];
                for (var j = 0; j < keys.length; j++) {
                    var val = row[keys[j]];
                    if (val != undefined && _TableFunctions.GetDataType(val) == 'Date') {
                        val = _TableFunctions.FormatDate(val);
                    }
                    val = val == undefined ? '' : val;
                    tab_text += '<td align="left">' + val + '</td>';
                }
                tab_text += '</tr>' + (i < data.length - 1 ? '<tr>' : '');
            }
            tab_text += '</table>';
            tab_text = tab_text.replace(/<A[^>]*>|<\/A>/g, '');//remove if u want links in your table
            tab_text = tab_text.replace(/<img[^>]*>/gi, ''); // remove if u want images in your table
            tab_text = tab_text.replace(/<input[^>]*>|<\/input>/gi, ''); // reomves input params

            var blob = new Blob([tab_text], { type: 'text/csv;charset=utf-8' }); //new way
            var now = Date.now();
            var fileName = 'Report ' + _TableFunctions.FormatDate(new Date(), 'dd-MM-yyyy hh:mm:ss') + '.xls';
            var csvUrl = URL.createObjectURL(blob);

            if (window.navigator.msSaveBlob) {
                // FOR IE BROWSER
                navigator.msSaveBlob(blob, fileName);
            } else {
                // FOR OTHER BROWSERS
                var link = document.createElement("a");
                link.href = csvUrl;
                link.style = "visibility:hidden";
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        }
    },
    FormatDate: function (date, format) {
        var month = '' + (date.getMonth() + 1),
            day = '' + date.getDate(),
            year = date.getFullYear(),
            hour = '' + date.getHours(),
            min = '' + date.getMinutes(),
            sec = '' + date.getSeconds();

        if (month.length < 2) month = '0' + month;
        if (day.length < 2) day = '0' + day;
        if (hour.length < 2) hour = '0' + hour;
        if (min.length < 2) min = '0' + min;
        if (sec.length < 2) sec = '0' + sec;
        var result = '';
        if (format) {
            result = format;
            result = result.replace('dd', day);
            result = result.replace('MM', month);
            result = result.replace('yyyy', year);
            result = result.replace('hh', hour);
            result = result.replace('mm', min);
            result = result.replace('ss', sec);
        }
        else {
            result = [day, month, year].join('/') + ' ' + [hour, min, sec].join(':');
        }
        return result;
    },
    DateFilterValue: function (value) {
        var date = value.split('/'),
            day = date[0],
            month = parseInt(date[1]) - 1,
            year = date[2];
        return [new Date(year, month, day, 0, 0, 0, 0), new Date(year, month, day, 23, 59, 59, 0)];
    },
    GetDataType: function (val) {
        var type = Object.prototype.toString.call(val);
        var result = '';
        switch (type) {
            case '[object String]': result = 'String'; break;
            case '[object Date]': result = 'Date'; break;
            case '[object Number]': result = 'Number'; break;
            default: break;
        }
        return result;
    },
    GetTableData: function (table) {
        var id = $(table).attr('data');
        var search = _TableFunctions.Data.filter(function (value) {
            return value.id == id;
        });
        var data = search.length == 0 ? null : search[0].data;
        if (data != null) {
            $(table).find('thead tr#filter td').each(function (i, e) {
                var name = $(e).attr('name');
                if (name != undefined && name != '') {
                    var sort = $(e).attr('sort'),
                        filter = $(e).find('input').val();
                    if (filter != undefined && filter != '') {
                        data = data.filter(function (value) {
                            var result = true,
                                val = _TableFunctions.GetDataValue(value, name),
                                type = _TableFunctions.GetDataType(val);
                            switch (type) {
                                case 'Date':
                                    var date = _TableFunctions.DateFilterValue(filter);
                                    result = val >= date[0] && val <= date[1];
                                    break;
                                default: result = val.toString().toLowerCase().indexOf(filter.toString().toLowerCase()) > -1; break;
                            }
                            return result;
                        });
                    }
                }
            });
        }
        return data;
    },
    GetTableInput: function (table) {
        var id = $(table).attr('data');
        var search = _TableFunctions.Input.filter(function (value) {
            return value.id == id;
        });
        return search.length == 0 ? null : search[0].input;
    },
    GetDataValue: function (data, name) {
        var names = name.split('.');
        var value = data;
        names.forEach(function (e, i) {
            if (value != undefined && value != null) {
                value = value[e];
            }
        });
        return value == undefined || value == null ? '' : value;
    },
    RefreshData: function () {
        _TableFunctions.Data.forEach(function (data, i) {
            if ($('table[data="' + data.id + '"]').length == 0)
                _TableFunctions.Data.splice(i, 1);
        })
    },
    SizeChange: function () {
        var size = $(this).val();
        _TableFunctions.GetParentTable(this);
        $(_TableFunctions.Parent).attr('dti', 0);
        _TableFunctions.RenderTableBody({
            table: _TableFunctions.Parent,
            size: size
        });
    },
    GetParentTable: function (e) {
        var $parent = $(e).parent();
        var name = $parent.tagName().toLowerCase();
        if (name == 'table')
            _TableFunctions.Parent = $parent;
        else if (name == 'body')
            _TableFunctions.Parent = null;
        else
            _TableFunctions.GetParentTable($parent);
    },
    GetCurrentSize: function (table) {
        var result = parseInt($(table).find('select#size').val());
        if (isNaN(result))
            result = $(table).find('tbody tr').length;
        return result;
    },
    DateFilterUpdate: function () {
        _TableFunctions.GetParentTable(this);
        _TableFunctions.RenderTableData(_TableFunctions.Parent);
        _TableFunctions.RenderTableFooter(_TableFunctions.Parent);
    },
    FilterUpdate: function (e) {
        var keyCode = e.which || e.keyCode;
        var key = e.key;
        var filter = false;
        if (keyCode != 229 && $.inArray(keyCode, [13]) !== -1) {
            filter = true;
        }
        else if (key && $.inArray(key, ["Enter"]) !== -1) {
            filter = true;
        }
        if (filter) {
            _TableFunctions.GetParentTable(this);
            _TableFunctions.RenderTableData(_TableFunctions.Parent);
            _TableFunctions.RenderTableFooter(_TableFunctions.Parent);
        }
    },
    RenderTableHeader: function (table) {
        var $thead = $(table).find('thead'),
            $filter = $thead.find('#filter');
        var data = _TableFunctions.GetTableData(table);
        if ($filter.length == 0)
            $thead.find('tr').attr('id', 'head');
        if (data.length > 0 && $filter.length == 0) {
            var filter = $('<tr id="filter"></tr>'),
                cols = $thead.find('#head td').length,
                buttons = $('<tr id="btns"><td colspan="' + cols + '"><span>Export: </span></td></tr>'),
                id = $(table).attr('data'),
                expBtn = $('<span class="table-export" data="' + id + '">Excel</span>'),
                exkeys = $(table).attr('exkeys');
            $thead.find('tr#head td').each(function (i, e) {
                var name = $(e).attr('name'),
                    val = data[0][name],
                    isNumber = false,
                    isDate = false,
                    td = $('<td ' + (name ? 'name="' + name + '"' : '') + '></td>');
                if (name) {
                    var input = $('<input type="text" />');
                    if (_TableFunctions.GetDataType(val) == 'Date') {
                        input.addClass('datepicker');
                        input.on('change', _TableFunctions.DateFilterUpdate);
                    }
                    input.on('keydown', _TableFunctions.FilterUpdate);
                    td.append(input);
                }
                filter.append(td);
            });
            buttons.find('td').append(expBtn);
            if (exkeys != undefined && exkeys != '')
                $thead.prepend(buttons);
            $thead.append(filter);
        }
    },
    RenderTableFooter: function (table, size) {
        var $thead = $(table).find('thead');
        var index = parseInt($(table).attr('dti'));
        if (isNaN(index))
            index = 1;
        else
            index++;
        if (!size) {
            var val = $(table).find('tfoot select#size').val();
            if (val != undefined) {
                size = parseInt(val);
            }
            else {
                size = 10;
            }
        }
        var cols = $thead.find('tr#head td').length;
        var data = _TableFunctions.GetTableData(table);
        var total = data == null ? 0 : data.length;
        $(table).find('tfoot').remove();
        $(table).append('<tfoot>' +
                        '<tr>' +
                            '<td colspan="' + cols + '">' +
                                '<div id="left">' +
                                    '<select id="size">' +
                                        '<option value="10"' + (size == 10 ? ' selected' : '') + '>10</option>' +
                                        '<option value="20"' + (size == 20 ? ' selected' : '') + '>20</option>' +
                                        '<option value="50"' + (size == 50 ? ' selected' : '') + '>50</option>' +
                                        '<option value="100"' + (size == 100 ? ' selected' : '') + '>100</option>' +
                                    '</select>' +
                                '</div>' +
                                '<div id="right">' +
                                    '<i id="next"></i>' +
                                    '<span id="total">' + (total == 0 ? 1 : Math.ceil(total / size)) + '</span><span>/</span><span id="current">' + index + '</span>' + //(total - index > size ? size : total - index)
                                    '<i id="previous"></i>' +
                                '</div>' +
                            '</td>' +
                        '</tr>' +
                    '</tfoot>');
        $(table).find('tfoot #right #next').on('click', function () {
            _TableFunctions.GetParentTable(this);
            _TableFunctions.ChangePage(true);
        });
        $(table).find('tfoot #right #previous').on('click', function () {
            _TableFunctions.GetParentTable(this);
            _TableFunctions.ChangePage(false);
        });
    },
    RenderTableBody: function (data) {
        if (!data.table || !data.size)
            return false;
        _TableFunctions.RenderTableHeader(data.table);
        var $body = $(data.table).find('tbody');
        if ($body.length == 0) {
            $(data.table).append('<tbody></tbody>');
            $body = $(data.table).find('tbody');
        }
        var currentSize = $body.find('tr').length;
        var size = data.size;
        if (currentSize > size) {
            $body.find('tr').each(function (i, e) {
                if (i >= size) {
                    $(e).remove();
                }
            });
        }
        else if (currentSize < size) {
            var cols = $(data.table).find('thead tr#head').find('td').length;
            while (currentSize < size) {
                var tr = document.createElement('tr');
                for (var i = 0; i < cols; i++) {
                    $(tr).append('<td></td>');
                }
                $body.append(tr);
                currentSize++;
            }
            _TableFunctions.RenderTableData(data.table);
        }
        _TableFunctions.RenderTableFooter(data.table, data.size);
        $(data.table).find('select#size').on('change', _TableFunctions.SizeChange);
    },
    RenderTableData: function (table) {
        if (!table)
            return false;
        var data = _TableFunctions.GetTableData(table);
        if (data != null) {
            var index = parseInt($(table).attr('dti')),
                size = _TableFunctions.GetCurrentSize(table),
                input = _TableFunctions.GetTableInput(table);
            if (isNaN(index)) {
                index = 0;
                $(table).attr('dti', index);
            }
            if (index * size > data.length) {
                index = index <= 0 ? 0 : index - 1;
                $(table).attr('dti', index);
            }
            else {
                $(table).find('tfoot #current').html(index + 1);
                $(table).find('tbody tr').each(function (i, e) {
                    _TableFunctions.RenderRowData(table, e, data, input, index * size, i);
                });
                if (_TableFunctions.OnRenderData)
                    _TableFunctions.OnRenderData($(table));
            }
        }
    },
    ShowDetail: function () {
        _TableFunctions.GetParentTable(this);
        var table = _TableFunctions.Parent;
        var data = _TableFunctions.GetTableData(table);
        var $form = $('form[name="' + $(table).attr('id') + '"]');
        if (data != null && $form.length > 0) {
            var tr = $(this).parent();
            var index = parseInt($(table).attr('dti'));
            var row = parseInt(tr.attr('dti'));
            if (isNaN(index) || isNaN(row)) {
                _TableFunctions.RenderTableData(table);
                return;
            }
            var rowData = data[index * _TableFunctions.GetCurrentSize(table) + row];
            $form.find('input').each(function (i, e) {
                var name = $(e).attr('name'),
                    type = $(e).attr('vtp');
                var val = rowData[name];
                if (val != undefined) {
                    var html = $('<div></div>').html(val).html();
                    $(e).val(html);
                    if (type == 'img') {
                        $('[for="' + $(e).attr('id') + '"]').css({
                            'background-image': "url('" + _TableFunctions.DefaultImage + "')"
                        });
                        var img = new Image();
                        img.onload = function () {
                            $('[for="' + $(e).attr('id') + '"]').css({
                                'background-image': "url('" + val + "')"
                            });
                        };
                        img.src = val;
                    }
                }
            });
            $form.find('select').each(function (i, e) {
                var name = $(e).attr('name');
                var val = rowData[name];
                if (val != undefined) {
                    var html = $('<div></div>').html(val).html();
                    $(e).val(val);
                }
            });
            $form.find('textarea').each(function (i, e) {
                var name = $(e).attr('name');
                var val = rowData[name];
                if (val != undefined) {
                    if (CKEDITOR && CKEDITOR.instances[name]) {
                        var html = $('<div></div>').html(val);
                        CKEDITOR.instances[name].setData(html[0].innerText);
                    }
                    $(e).val(val);
                }
            });
        }
    },
    RenderRowData: function (table, tr, data, input, index, row) {
        if (index + row < data.length) {
            var rowData = data[index + row],
                view = $(table).attr('view'),
                update = $(table).attr('update'),
                remove = $(table).attr('remove'),
                key = $(table).attr('key'),
                keys = $(table).attr('keys'),
                title = $(table).attr('ptitle'),
                color = $(table).attr('color');
            $(tr).attr('dti', row);
            $(tr).addClass('data');
            if (color && color != '') {
                $(tr).attr('color', _TableFunctions.GetDataValue(rowData, color));
            }
            $(tr).find('td').each(function (j, el) {
                if (input != null && j == 0) {
                    $(el).html('');
                    input.forEach(function (data, k) {
                        var html = data.html;
                        if (keys) {
                            keys = keys.split(',');
                            keys.forEach(function (key, m) {
                                while (html.indexOf('_' + key) != -1) {
                                    html = html.replace('_' + key, _TableFunctions.GetDataValue(rowData, key));
                                }
                            });
                        }
                        $(el).append(html);
                        if (data.event) {
                            data.event.forEach(function (event, l) {
                                $(el).find('#' + event.id).on(event.key, event.handler);
                            });
                        }
                    });
                }
                else if ((view || update || remove) && key && j == 0) {
                    var html = '';
                    if (view) {
                        view = view.replace('%7B' + key + '%7D', _TableFunctions.GetDataValue(rowData, key));
                        html += '<a href="' + view + '" target="_self" for="pop-up-content" ptitle="' + title + '" changeurl="false" class="button-icon view"></a>';
                    }
                    if (update) {
                        update = update.replace('%7B' + key + '%7D', _TableFunctions.GetDataValue(rowData, key));
                        html += '<a href="' + update + '" target="_self" for="pop-up-content" ptitle="' + title + '" class="button-icon edit"></a>';
                    }
                    if (remove) {
                        remove = remove.replace('%7B' + key + '%7D', _TableFunctions.GetDataValue(rowData, key));
                        var name = _TableFunctions.GetDataValue(rowData, 'Name');
                        html += '<a href="' + remove + '" target="_self" class="button-icon remove" name="' + name + '"></a>';
                    }
                    $(el).html('<div>' + html + '</div>');
                    $(table).attr('init', '1');
                    $(el).find('a.remove').on('click', function () {
                        var url = $(this).attr('href'),
                            name = $(this).attr('name');
                        $('#pop-up-content').html('<div><p>Bạn muốn xóa ' + name + '?</p><div class="text-right"><a class="button remove" for="main-content" ptitle="' + title + '" changeurl="false" href="' + url + '">Xóa</a><span class="button cancel">Hủy</span></div></div>');
                        $('#pop-up-content').find('span.cancel').on('click', function () {
                            $('#pop-up').removeClass('active');
                        });
                        $('#pop-up').addClass('active');
                        return false;
                    });
                }
                else {
                    var head = $(table).find('thead tr#head td')[j],
                        descAttr = $(head).attr('description'),
                        nameAttr = $(head).attr('name'),
                        text = $(head).attr('text'),
                        data = nameAttr == undefined ? '' : _TableFunctions.GetDataValue(rowData, nameAttr),
                        dataType = _TableFunctions.GetDataType(data),
                        desc = descAttr == undefined ? '' : _TableFunctions.GetDataValue(rowData, descAttr);
                    if (dataType == 'Date') {
                        data = _TableFunctions.FormatDate(data);
                    }
                    $(el).html('<div ' + (text ? 'text="' + text + '"' : '') + '>' + data + '</div>' + (desc == '' ? '' : '<div class=\"desc\">' + desc + '</div>'));
                    $(el).on('click', _TableFunctions.ShowDetail);
                }
            });
        }
        else {
            $(tr).attr('dti', '');
            $(tr).removeClass('data');
            $(tr).find('td').html('');
        }
    },
    ChangePage: function (next) {
        var table = _TableFunctions.Parent,
            index = parseInt($(table).attr('dti'));
        if (isNaN(index))
            index = next ? 1 : 0;
        else if (next)
            index++;
        else
            index = index <= 0 ? 0 : index - 1;
        $(table).attr('dti', index);
        _TableFunctions.RenderTableData(table);
        if (ProcessLinks)
            ProcessLinks(table);
    },
    JsonParse: function (key, value) {
        if (typeof value === "string" && _TableFunctions.DateFormat.test(value)) {
            return new Date(parseInt(value.substr(6)));
        }
        return value;
    }
};