var _inSelection = false;
_cForm = {
    Init: function (elm) {
        $(elm).find('label[for]').each(function (i, e) {
            var $input = $('input[name="' + $(e).attr('for') + '"]');
            if ($input) {
                $input.on('focus', function () {
                    $(this).parent().addClass('focus');
                });
                $input.on('focusout', function () {
                    _cForm.ValidateInput(this);
                    $(this).parent().removeClass('focus');
                });
            }
            $(e).on('click', function () {
                var name = $(this).attr('for');
                var $input = $('input[name="' + name + '"]');
                $(this).parent().addClass('focus');
                if ($input.attr('data-dtp') != 'ddl')
                    $input.focus();
                else {
                    $('input[name="dpl-' + name + '"]').focus();
                }
            });
        });
        $(elm).find('input[data-dtp="nbr"]').each(function (i, e) {
            $(e).keydown(function (e) {
                // Allow: backspace, delete, tab, escape, enter and .
                if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
                    // Allow: Ctrl+A, Command+A
                    (e.keyCode == 65 && (e.ctrlKey === true || e.metaKey === true)) ||
                    // Allow: home, end, left, right, down, up
                    (e.keyCode >= 35 && e.keyCode <= 40)) {
                    // let it happen, don't do anything
                    return;
                }
                // Ensure that it is a number and stop the keypress
                if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                    e.preventDefault();
                }
            });
        });
        $(elm).find('input[data-dtp="ddl"]').each(function (i, e) {
            var name = $(e).attr('name');
            if ($('input[name="dpl-' + name + '"]').length == 0) {
                $(e).addClass('hidden');
                var input = _cForm.CreateNewInput([
                    ['type', 'text'],
                    ['name', 'dpl-' + name]
                ]);
                $(input).on('click', function () {
                    var $parent = $(this).parent()
                    $parent.toggleClass('focus');
                    if (!$parent.hasClass('focus')) {
                        $(this).blur();
                    }
                });
                $(input).on('focus', function () {
                    var name = $(this).attr('name').substring(4);
                    $(this).attr({
                        'readonly': 'readonly'
                    });
                    $('label[for="' + name + '"]').addClass('hidden');
                    $('.select[data-for="' + name + '"]').addClass('focus');
                });
                $(input).on('focusout', function () {
                    if (!_inSelection) {
                        $(this).parent().removeClass('focus');
                        $(this).removeAttr('readonly');
                        var name = $(this).attr('name').substring(4);
                        $('.select[data-for="' + name + '"]').removeClass('focus');
                        var $input = $('input[name="' + name + '"]');
                        $input.val($(this).val());
                        _cForm.ValidateInput($input);
                    }
                });
                $(e).parent().append(input);
            }
        });
        $(elm).find('input[type="checkbox"]').each(function (i, e) {
            var name = $(e).attr('name');
            if ($('i.check[data-name="' + name + '"]').length == 0) {
                var $parent = $(e).parent(),
                    check = document.createElement('i');
                $(check).attr({
                    'data-name': name,
                    'class': 'check'
                });
                $(check).on('click', function () {
                    $(this).toggleClass('checked');
                    if ($(this).hasClass('checked')) {
                        var input = $('input[name="' + $(this).attr('data-name') + '"]');
                        input.prop('checked', true);
                        input.val('true');
                    }
                    else
                        $('input[name="' + $(this).attr('data-name') + '"]').prop('checked', false);
                });
                $parent.prepend(check);
                $('label[for="' + name + '"]').on('click', function () {
                    var name = $(this).attr('for'),
                        $input = $('input[name="' + name + '"]'),
                        $check = $('i.check[data-name="' + name + '"]');
                    $check.toggleClass('checked');
                    if ($check.hasClass('checked')){
                        $input.prop('checked', true);
                        $input.val('true');
                    }
                    else
                        $input.prop('checked', false);
                });
            }
        });
        $(elm).find('.select[data-for]').each(function (i, e) {
            $(e).on('mouseenter', function () { _inSelection = true; });
            $(e).on('mouseleave', function () { _inSelection = false; });
            $(e).find('.option').each(function (i, e) {
                var selected = $(e).attr('selected');
                $(e).on('click', function (s) {
                    var name = $(this).parent().attr('data-for');
                    var $input = $('input[name="' + name + '"]'),
                        $display = $('input[name="dpl-' + name + '"]'),
                        html = $(this).html(),
                        value = $(this).attr('data-value');
                    $(this).parent().find('.option[selected]').each(function (i, e) {
                        $(e).removeAttr('selected');
                    });
                    $(this).attr('selected', 'true');
                    $input.val(value ? value : html);
                    $display.val(html);
                    $(this).parent().removeClass('focus');
                    $(this).parent().parent().removeClass('focus');
                    _cForm.ValidateInput($input);
                });
                if (selected)
                    $(e).click();
            });

        });
    },
    ClearForm: function (elm) {
        $(elm).find('.focus').removeClass('focus');
        $(elm).find('input').each(function (i, e) {
            if ($(e).attr('type') != 'checkbox')
                $(e).val('');
            else {
                $(e).prop('checked', false);
                $('i.check[data-name="' + $(e).attr('name') + '"]').removeClass('checked');
            }
            //_cForm.ValidateInput(e);
        });
    },
    CreateNewInput: function (info) {
        var display = document.createElement('input');
        for (var i = 0; i < info.length; i++) {
            display.setAttribute(info[i][0], info[i][1]);
        }
        return display;
    },
    IsEmail: function (email) {
        var regex = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
        return regex.test(email);
    },
    IsNumberic: function (str) {
        var regex = /[^0-9]/;
        return !regex.test(str);
    },
    ValidateInput: function (e) {
        var val = $(e).val(),
            name = $(e).attr('name'),
            $label = $('label[for="' + name + '"]');
        if (val == '') {
            $(e).parent().removeAttr('data-vld');
            $label.removeClass('hidden');
            if ($('input[name="' + name + '"]').attr('data-rqd') == 'true') {
                _cForm.ShowErrorMsg($label.parent(), 'The ' + $label.html() + ' field is required.');
            }
            else
                $label.parent().find('i.val').remove();
        }
        else {
            var dtp = $(e).attr('data-dtp');
            var valid = 'valid',
                invalid = 'invalid',
                res = valid;
            if (!dtp) {
                switch (dtp) {
                    case 'eml': if (!_cForm.IsEmail(val)) res = invalid; break;
                    case 'nbr': if (!_cForm.IsNumberic(val)) res = invalid; break;
                    default: break;
                }
            }
            $(e).parent().attr('data-vld', res);
            if (res == invalid) {
                _cForm.ShowErrorMsg($label.parent(), 'The ' + $label.html() + ' field is invalid.');
            }
            else
                $label.parent().find('i.val').remove();
        }
    },
    ShowErrorMsg: function (e, val) {
        var $msg = $(e).find('i.val[type="er"]');
        if ($msg.length == 0) {
            var msg = document.createElement('i');
            $msg = $(msg);
            $msg.attr({
                'class': 'val',
                'type': 'er'
            });
        }
        $msg.html(val);
        $(e).append($msg);
    }
};