document.addEventListener('DOMContentLoaded', function () {
    AjaxMemberList()

    setInterval(() => {
        if ($(window).innerWidth() > 975 && !$('body').hasClass('sb-sidenav-toggled')) {
            $('#ctrlBtn').css({ transition: 'margin-left .1s ease-out', marginLeft: '0' })
        } else if ($(window).innerWidth() > 800 && !$('body').hasClass('sb-sidenav-toggled')) {
            $('#ctrlBtn').css({ transition: 'margin-left .1s ease-out', marginLeft: '225px' })
        } else {
            $('#ctrlBtn').css({ transition: 'margin-left .1s ease-out', marginLeft: '0' })
        }
    }, 10)
    $(window).scroll(() => {
        if ($(this).scrollTop() > 175) {
            $('#ctrlBtn1').addClass('btn-outline-primary').removeClass('btn-primary')
            $('#ctrlBtn2').addClass('btn-outline-warning').removeClass('btn-warning')
            $('#ctrlBtn3').addClass('btn-outline-danger').removeClass('btn-danger')
        } else {
            $('#ctrlBtn1').addClass('btn-primary').removeClass('btn-outline-primary')
            $('#ctrlBtn2').addClass('btn-warning').removeClass('btn-outline-warning')
            $('#ctrlBtn3').addClass('btn-danger').removeClass('btn-outline-danger')
        }
        if ($(this).scrollTop() > 225) {
            $('#cardHeader').addClass('py-1')
            $('#ListHead tr th').css('top', '87px')
        } else if ($(this).scrollTop() < 215) {
            $('#cardHeader').removeClass('py-1')
            $('#ListHead tr th').css('top', '103px')
        }
    })
    $(document).on('click', '.dropdown-menu', function (e) {
        e.stopPropagation()
    }).on('mouseleave', '.dropdown-menu', function () {
        $('body').click()
    }).on('click', '.btn, .page-item', function () {
        $(this).css({
            'outline': 'none !important',
            'box-shadow': 'none'
        })
    })
    ResizeInitial()
    $(window).resize(ResizeInitial)

    $('#ctrlSelect').change(function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val($(this).val())
        AjaxMemberList()
    })

    $('#ctrlSearch').keypress(function (e) {
        if (e.keyCode === 13) {
            e.preventDefault()
            $('#ctrlSubmit').click()
        }
    })

    $('#ctrlBtn1').click(function () {
        let info = ''
        for (let e of emails) {
            info += `<p class="text-success" style="cursor: pointer"><i class="far fa-bell"></i> ${e}</p>`
        }
        if (!emails.length) {
            info += `<p class="text-success" style="cursor: pointer"><i class="far fa-bell"></i> To: 所有人</p>`
        }
        let html = (`
            <form id="AjaxBoxForm">
                <div class="form-group">
                    <label for="AjaxBoxTextarea">系統通知:</label>
                    <textarea id="AjaxBoxTextarea" class="form-control" rows="3"></textarea>
                </div>
            </form>
            <p>發送的對象:</p>
            ${info}
        `)
        let target = members.length ? `選取的 ${members.length} 個會員` : `所有會員`
        $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
        $('#AjaxBoxBody').html(html)
        $('#AjaxBoxTitle').html(`<i class="far fa-bell"></i> 發送系統通知 To: ${target}`)
        $('#OK').off('click').on('click', function () {
            if ($('#AjaxBoxTextarea').val().trim() === '') {
                alert('請填寫系統通知內文')
            } else if (confirm('確定要送出系統通知嗎?')) {
                $.ajax({
                    url: '/BackEndMember/SendMessageAsync',
                    type: 'post',
                    data: {
                        members: members.join(),
                        message: $('#AjaxBoxTextarea').val().trim()
                    },
                    success: function (data) {
                        alert('發送成功!')
                        members.length = 0
                        emails.length = 0
                        AjaxMemberList()
                    }
                })
                $(this).prev().click()
            }
        })
    })

    $('#ctrlBtn2a').click(function () {
        $('#ctrlBtn2b').prop('checked', false)
        $('#ctrlBtn2c').prop('checked', false)
        $('#ctrlBtn2d').prop('checked', false)
        $('#ctrlBtn2e').prop('checked', false)
        $('#fPageCurrent').val(1)
        $('#fSortRule').val("0")
        $('#fMemberRoleInfo').val("0")
        $('#fMemberRoleIsBan').val(false)
        AjaxMemberList()
        $(window).scrollTop(225)
        $('#ctrlHint0').text('查詢所有會員').css('color', 'black')
        $('#ctrlHint1').text('')
        MemberRoleInfoFont($('#fSortRule').val())
    })

    $('#ctrlBtn2b,#ctrlBtn2c,#ctrlBtn2d').click(function () {
        $('#ctrlBtn2a').prop('checked', false)
        $('#fPageCurrent').val(1)
        $('#fSortRule').val("0")
        $('#fMemberRoleInfo').val(`${$('#ctrlBtn2d').prop('checked') ? '1' : ''}${$('#ctrlBtn2c').prop('checked') ? '2' : ''}${$('#ctrlBtn2b').prop('checked') ? '3' : ''}`)
        AjaxMemberList()
        $(window).scrollTop(225)
        let flag = $('#ctrlBtn2b').prop('checked') === $('#ctrlBtn2c').prop('checked') && $('#ctrlBtn2c').prop('checked') === $('#ctrlBtn2d').prop('checked')
        $('#ctrlHint0').text($('#ctrlBtn2e').prop('checked') ? '查詢停權會員' : '查詢所有會員')
            .css('color', $('#ctrlBtn2e').prop('checked') ? 'red' : 'black')
        $('#ctrlHint1').text(flag ? '' :
            `查詢: ${$('#ctrlBtn2d').prop('checked') ? '未驗證 ' : ''}${$('#ctrlBtn2c').prop('checked') ? '普通會員 ' : ''}${$('#ctrlBtn2b').prop('checked') ? '商家 ' : ''}`)
        MemberRoleInfoFont($('#fSortRule').val())
    })

    $('#ctrlBtn2e').click(function () {
        $('#ctrlBtn2a').prop('checked', false)
        $('#fPageCurrent').val(1)
        $('#fSortRule').val("0")
        $('#fMemberRoleIsBan').val($('#ctrlBtn2e').prop('checked'))
        AjaxMemberList()
        $(window).scrollTop(225)
        let flag = $('#ctrlBtn2b').prop('checked') === $('#ctrlBtn2c').prop('checked') && $('#ctrlBtn2c').prop('checked') === $('#ctrlBtn2d').prop('checked')
        $('#ctrlHint0').text($('#ctrlBtn2e').prop('checked') ? '查詢停權會員' : '查詢所有會員')
            .css('color', $('#ctrlBtn2e').prop('checked') ? 'red' : 'black')
        $('#ctrlHint1').text(flag ? '' :
            `查詢: ${$('#ctrlBtn2d').prop('checked') ? '未驗證 ' : ''}${$('#ctrlBtn2c').prop('checked') ? '普通會員 ' : ''}${$('#ctrlBtn2b').prop('checked') ? '商家 ' : ''}`)
        MemberRoleInfoFont($('#fSortRule').val())
    })

    $('#ctrlBtn3').click(function () {
        let tomorrow = `${new Date().getFullYear()}-${String(new Date().getMonth() + 1).padStart(2, '0')}-${String(new Date().getDate() + 1).padStart(2, '0')}`
        if (!members.length) {
            alert('沒有選取會員')
            return
        }
        let info = ''
        for (let e of emails) {
            info += `<p class="text-danger" style="cursor: pointer"><i class="fas fa-ban"></i> ${e}</p>`
        }
        let html = (`
            <form id="AjaxBoxForm">
                <div class="form-group">
                    <label for="AjaxBoxTextarea">停權原因:</label>
                    <textarea id="AjaxBoxTextarea" class="form-control" rows="3"></textarea>
                </div>
                <div class="form-group">
                    <label for="AjaxBoxDate">停權到期日:</label>
                    <input type="date" id="AjaxBoxDate" class="form-control" style="cursor: pointer" value="${tomorrow}" min="${tomorrow}">
                </div>
            </form>
            <p>停權的對象:</p>
            ${info}
        `)
        $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
        $('#AjaxBoxBody').html(html)
        $('#AjaxBoxTitle').html(`停權會員: 選取的 ${members.length} 個會員`)
        $('#OK').off('click').on('click', function () {
            if ($('#AjaxBoxTextarea').val().trim() === '') {
                alert('請填寫停權原因')
            } else if (confirm(`確定要停權選取的 ${members.length} 個會員嗎?`)) {
                $.ajax({
                    url: '/BackEndMember/MultiBanMemberAsync',
                    type: 'post',
                    data: {
                        members: members.join(),
                        reason: $('#AjaxBoxTextarea').val().trim(),
                        endtime: $('#AjaxBoxDate').val()
                    },
                    success: function (data) {
                        alert('停權成功!')
                        members.length = 0
                        emails.length = 0
                        AjaxMemberList()
                    }
                })
                $(this).prev().click()
            }
        })
    })

    $('#ctrlSubmit').click(function () {
        $('#fPageCurrent').val(1)
        $('#fSortRule').val("0")
        $('#fKeyword').val($('#ctrlSearch').val().toLowerCase().trim())
        AjaxMemberList()
        $(window).scrollTop(225)
        $('#ctrlHint2').text($('#fKeyword').val() ? `關鍵字: ${$('#fKeyword').val()}` : '')
        MemberRoleInfoFont($('#fSortRule').val())
    })

    $('#ctrlReset').click(function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#fKeyword').val('')
        $('#fSortRule').val(0)
        $('#fMemberRoleInfo').val("0")
        $('#fMemberRoleIsBan').val(false)
        $('#ctrlBtn2a').prop('checked', false)
        $('#ctrlBtn2b').prop('checked', false)
        $('#ctrlBtn2c').prop('checked', false)
        $('#ctrlBtn2d').prop('checked', false)
        $('#ctrlBtn2e').prop('checked', false)
        AjaxMemberList()
        $(window).scrollTop(225)
        $('#ctrlHint0').text('查詢所有會員').css('color', 'black')
        $('#ctrlHint1').text('')
        $('#ctrlHint2').text('')
        MemberRoleInfoFont($('#fSortRule').val())
        $('#ctrlSelect').prop('selectedIndex', 0)
    })

    $('#ctrlPage1,#ctrlPage2').on('click', '.customGotoPage', function () {
        $('#fPageCurrent').val(parseInt($(this).data('id')))
        AjaxMemberList()
        $(window).scrollTop(225)
    }).on('click', '.customGotoNewPage', function () {
        let page = prompt('請輸入頁碼', Math.floor(Math.random() * maxpage) + 1)
        if (page !== null && /^(?=.*[1-9])[0-9]+$/.test(page)) {
            $('#fPageCurrent').val(Math.min(parseInt(page), maxpage))
            AjaxMemberList()
            $(window).scrollTop(225)
        }
    })

    $('#ListHead0').click(function () {
        
    })

    $('#ListHead1').click(function () {
        $('#fPageCurrent').val(1)
        $('#fSortRule').val($('#fSortRule').val() === '1a' ? '1d' : '1a')
        AjaxMemberList()
        $(window).scrollTop(225)
        MemberRoleInfoFont($('#fSortRule').val())
    })

    $('#ListHead2').click(function () {
        $('#fPageCurrent').val(1)
        $('#fSortRule').val($('#fSortRule').val() === '2a' ? '2d' : '2a')
        AjaxMemberList()
        $(window).scrollTop(225)
        MemberRoleInfoFont($('#fSortRule').val())
    })

    $('#ListHead3').click(function () {
        $('#fPageCurrent').val(1)
        $('#fSortRule').val($('#fSortRule').val() === '3a' ? '3d' : '3a')
        AjaxMemberList()
        $(window).scrollTop(225)
        MemberRoleInfoFont($('#fSortRule').val())
    })

    $('#ListHead4').click(function () {
        $('#fPageCurrent').val(1)
        $('#fSortRule').val($('#fSortRule').val() === '4a' ? '4d' : '4a')
        AjaxMemberList()
        $(window).scrollTop(225)
        MemberRoleInfoFont($('#fSortRule').val())
    })

    $('#ListHead5').click(function () {
        $('#fPageCurrent').val(1)
        $('#fSortRule').val($('#fSortRule').val() === '5a' ? '5d' : '5a')
        AjaxMemberList()
        $(window).scrollTop(225)
        MemberRoleInfoFont($('#fSortRule').val())
    })
})