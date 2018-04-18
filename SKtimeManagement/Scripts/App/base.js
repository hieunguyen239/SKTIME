var _history = window.history && window.history.pushState;
var firstChangeUrl = true;
if (_history) {
    $(window).on('popstate', function (e) {
        var state = e.originalEvent.state;
        //console.log(e.originalEvent);
        if (state && state.display && state.html) {
            $(state.display).html(state.html);
            $('title').html(state.title);
        }
    });
}
Date.prototype.today = function () {
    return ((this.getDate() < 10) ? "0" : "") + this.getDate() + "/" + (((this.getMonth() + 1) < 10) ? "0" : "") + (this.getMonth() + 1) + "/" + this.getFullYear();
}
Date.prototype.timeNow = function () {
    return ((this.getHours() < 10) ? "0" : "") + this.getHours() + ":" + ((this.getMinutes() < 10) ? "0" : "") + this.getMinutes() + ":" + ((this.getSeconds() < 10) ? "0" : "") + this.getSeconds();
}
function Loading(el) {
    var absolute = true;
    if (!el || el.length == 0) {
        absolute = false;
        el = $('body');
    }
    var load = $('<div class="loading"></div>');
    if (absolute)
        load.addClass('absolute');
    load.append('<span class="centerer"></span><div class="centered uil-ring-css"><div></div></div>');
    $(el).append(load);
}
function Loaded(el) {
    if (!el || el.length == 0)
        el = $('body');
    $(el).find('> .loading').remove();
}
function AjaxRequest(data) {
    $.ajax({
        url: data.url,
        dataType: "json",
        type: data.type ? data.type : 'GET',
        data: data.data,
        async: true,
        contentType: false,
        processData: data.processData ? true : data.processData,
        cache: false,
        xhr: function () {
            var xhr = new window.XMLHttpRequest();
            //Upload progress
            if (data.uploadProgress) {
                xhr.upload.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        var percentComplete = evt.loaded / evt.total;
                        data.uploadProgress(percentComplete);
                    }
                }, false);
            }
            //Download progress
            if (data.downloadProgress) {
                xhr.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        var percentComplete = evt.loaded / evt.total;
                        data.downloadProgress(percentComplete);
                    }
                }, false);
            }
            return xhr;
        },
        beforeSend: data.beforeSend ? data.beforeSend : function () {
            Loading();
        },
        complete: data.complete ? data.complete : function () {
            Loaded();
        },
        success: data.success,
        error: data.error
    });
}
function ChangeUrl(title, url, state) {
    if (_history)
    {
        if (firstChangeUrl) {
            window.history.pushState({
                display: '#main-content',
                html: $('#main-content').html(),
                title: $('title').html()
            }, $('title').html(), window.location.pathname);
            firstChangeUrl = false;
        }
        $('title').html(title);
        window.history.pushState(state, title, url);
    }
}
function ProcessForms(el) {
    $(el).find('.date').each(function (i, e) {
        var format = $(e).attr('format');
        if (format == undefined || format == '')
            format = _DateTimeFormatString;
        $(e).datetimepicker({
            format: format
        });
    });
    $(el).find('form').each(function (i, form) {
        var url = $(form).attr('action'),
            type = $(form).attr('method'),
            display = $(form).attr('for'),
            fail = $(form).attr('fail'),
            changeUrl = $(form).attr('url'),
            keepPopup = $(form).attr('keep-pop-up') == 'true',
            formData = $(form).serialize();
        if (display)
            display = '#' + display;
        if (fail)
            fail = '#' + fail;
        $(form).find('input[type="submit"]').on('click', function () {
            var btnUrl = $(this).attr('url'),
                title = $('title').html();
            try {
                if (type.toLowerCase() == 'get') {
                    if (formData != '')
                        return url += formData;
                } else {
                    formData = new FormData();
                    $.each($(form).serializeArray(), function (i, field) {
                        //console.log(field.name + ', ' + field.value);
                        formData.append(field.name, field.value);
                    });
                }
                AjaxRequest({
                    url: url,
                    data: formData,
                    type: type,
                    processData: type.toLowerCase() == 'post' ? false : true,
                    success: function (data) {
                        if (data.result) {
                            if (changeUrl)
                                window.location = changeUrl;
                            if (data.redirect)
                                window.location = data.redirect;
                        }
                        else if (data.message) {
                            $(form).find('#form-message').html(data.message);
                        }
                        if (display && data.html) {
                            if (data.result == false && fail)
                                display = fail;
                            $(display).html(data.html);
                            ProcessLinks(display);
                            ProcessForms(display);
                            if (display == '#pop-up-content') {
                                $('#pop-up').addClass('active');
                                $('body').addClass('locked');
                            } else if (!keepPopup) {
                                $('#pop-up').removeClass('active');
                                $('body').removeClass('locked');
                            }
                        }
                        if (data.result && btnUrl) {
                            ChangeUrl(title, btnUrl, {
                                display: '#main-content',
                                html: data.html,
                                title: title
                            });
                        }
                    },
                    error: function (er) {
                        console.log(er);
                    }
                });
                return false;
            }
            catch (e) {
                return false;
            }
        });
        $(form).find('input[type="file"][url]').on('change', function () {
            var files = this.files,
                img = $(this).attr('name'),
                folder = $(this).attr('folder'),
                url = $(this).attr('url'),
                input = $(this).attr('for');
            for (var i = 0; i < files.length; i++) {
                var data = new FormData();
                data.append('folder', folder);
                data.append(files[i].name, files[i]);
                AjaxRequest({
                    url: url,
                    type: 'post',
                    data: data,
                    processData: false,
                    success: function (data) {
                        $('img[for="' + img + '"]').attr('src', data.img);
                        $('input[name=' + input + ']').val(data.file);
                        //console.log('saved');
                    },
                    uploadProgress: function (progress) {
                        //console.log(progress);
                    }
                });
            }
        });
        ProcessSelects(form);
    });
}
function ProcessSelects(el) {
    $(el).find('select[data]').each(function (i, e) {
        var url = $(e).attr('data'),
            loaded = $(e).attr('load');
        if (loaded != '1') {
            $(e).attr('load', '1');
            AjaxRequest({
                url: url,
                type: 'post',
                processData: false,
                success: function (data) {
                    if (data.id && data.name) {
                        $(el).find('select[data="' + url + '"]').each(function (index, select) {
                            var value = $(select).attr('value');
                            data.id.forEach(function (id, j) {
                                $(select).append('<option value="' + id + '" ' + (id == value ? 'selected' : '') + '>' + data.name[j] + '</option>');
                            });
                        });
                    }
                    InitTagList({
                        el: e,
                        list: data
                    });
                    //console.log('saved');
                }
            });
        }
    });
}
function ProcessLinks(el) {
    $(el).find('a[for]').on('click', function () {
        try {
            var url = $(this).attr('href'),
                display = '#' + $(this).attr('for'),
                title = $(this).attr('ptitle'),
                keppPopup = $(this).attr('keeppopup') == 'true',
                noChangeUrl = $(this).attr('changeurl') == 'false';
            if (!title)
                title = '';
            AjaxRequest({
                url: url,
                type: 'GET',
                success: function (data) {
                    if (!noChangeUrl)
                        ChangeUrl(title, url, {
                            display: '#main-content',
                            html: data.html,
                            title: title
                        });
                    if (data.remove) {
                        $(display).remove();
                    }
                    else {
                        $(display).html(data.html);
                        if (!keppPopup) {
                            if (display == '#pop-up-content') {
                                $('#pop-up').addClass('active');
                                $('body').addClass('locked');
                            }
                            else {
                                $('#pop-up').removeClass('active');
                                $('body').removeClass('locked');
                            }
                        }
                        ProcessLinks(display);
                        ProcessForms(display);
                    }
                },
                error: function (e) {
                    //console.log(e);
                    window.location = url;
                }
            });
            return false;
        }
        catch (e) {
            return true;
        }
    });
    $('.tabs').each(function (i, e) {
        $(e).find('span[for]').on('click', function () {
            var id = $(this).attr('for');
            $(e).find('span[for]').each(function (j, el) {
                var currentID = $(el).attr('for');
                if (id == currentID) {
                    $(el).addClass('active');
                    $('#' + currentID).removeClass('hidden');
                }
                else {
                    $(el).removeClass('active');
                    $('#' + currentID).addClass('hidden');
                }
            });
        });
    });
}
function NewTag(data) {
    var list = data.list,
        removeUrl = data.removeUrl,
        display = data.display,
        tag = $('<span class="tag" value="' + data.value + '"><span id="remove">x</span><span id="name">' + data.name + '</span></span>');
    $('#' + display).append(tag);
    InitTagItem({
        el: tag,
        list: list,
        removeUrl: removeUrl
    });
    $(list).find('option[value="' + data.value + '"]').remove();
    $(list).val('');
    return tag;
}
function InitTagList(data) {
    var el = data.el,
        addUrl = $(el).attr('tag-add'),
        removeUrl = $(el).attr('tag-remove'),
        display = $(el).attr('for');
    if (addUrl && removeUrl && display) {
        if (data.list && data.list.tagged) {
            data.list.tagged.forEach(function (e, i) {
                if (e) {
                    NewTag({
                        list: el,
                        removeUrl: removeUrl,
                        display: display,
                        name: data.list.name[i],
                        value: data.list.id[i]
                    });
                }
            });
        }
        $(el).on('change', function () {
            var list = $(this),
                value = list.val();
            if (value != '') {
                var option = list.find('option[value="' + value + '"]'),
                    name = option.html();
                //console.log(addUrl.replace('__ID', value), value);
                _messageActive = true;
                AjaxRequest({
                    url: addUrl.replace('__ID', value),
                    success: function (data) {
                        if (data.result) {
                            NewTag({
                                list: list,
                                removeUrl: removeUrl,
                                display: display,
                                name: name,
                                value: value
                            });
                        }
                    }
                });
            }
        });
    }
}
function InitTagItem(d) {
    $(d.el).find('#remove').on('click', function () {
        var tag = $(this).parent(),
            value = tag.attr('value'),
            name = tag.find('#name').html();
        AjaxRequest({
            url: d.removeUrl.replace('__ID', value),
            success: function (data) {
                if (data.result) {
                    $(d.list).append('<option value="' + value + '">' + name + '</option>');
                    tag.remove();
                }
            }
        });
    });
}
function GetCurrencyString(d) {
    d = parseInt(d);
    var str = d.toString(), sign = '';
    if (str.startsWith('-')) {
        str = str.substring(1, str.length);
        sign = '-';
    }
    var index = str.length % 3, count = parseInt(str.length / 3);
    if (count == 0)
        return sign + str;
    else {
        var result = str.substring(0, index) + ',';
        result = result == ',' ? '' : result;
        for (var i = 0; i < count; i++) {
            result += str.substring(i * 3 + index, (i + 1) * 3 + index) + (i == count - 1 ? '' : ',');
        }
        return sign + result;
    }
}
function Print(divName) {
    var now = new Date();
    $('#' + divName + ' #date').html(now.today() + ' ' + now.timeNow());
    var printContents = '<div id="print">' + document.getElementById(divName).innerHTML + '</div>';
    $('#page-body').addClass('hidden');

    $('body').append(printContents);

    window.print();
    $('#print').remove();
    $('#page-body').removeClass('hidden');
}
String.prototype.replaceAll = function (search, replacement) {
    var target = this;
    return target.replace(new RegExp(search, 'g'), replacement);
};
function NumberInput(el) {
    $(el).find('.input-number').each(function (i, e) {
        var input = $(e).find('input[type="number"]'),
            edit = $('<span contenteditable="true" for="' + input.attr('id') + '">' + GetCurrencyString(input.val()) + '</span>');
        $(e).append(edit);
        $(e).find('[contenteditable="true"]').on('keyup', function () {
            var str = $(this).html(),
                val = parseInt(str.replaceAll(',', ''));
            if (!isNaN(val)) {
                input.val(val);
                input.trigger('change');
            }
            $(this).html(GetCurrencyString(input.val()));
            var range = document.createRange();  
            range.setStart(this, 1);
            var sel = window.getSelection();
            range.collapse(true);
            sel.removeAllRanges();
            sel.addRange(range);
        });
    });
}