let maxpage

function AjaxArticleList() {
    $('#ArticleManagement').addClass('d-none')
    $.ajax({
        url: $('#fType').val() === 'R' ? '/BackEndArticle/ReplyList' : '/BackEndArticle/ArticleList',
        type: 'post',
        data: $('#ctrlForm').serialize(),
        success: function (data) {
            if (data.length && data[0].ChangePage != 0) {
                $('#fPageCurrent').val(data[0].ChangePage)
            }
            maxpage = data.length ? Math.ceil(data[0].Count / parseInt($('#fPageSize').val())) : 1
            $('#ctrlHint').text($('#fKeyword').val() == '' ? '' : `關鍵字: ${$('#fKeyword').val()}`)
            $('#ListBody').empty()
            $.each(data, function (i, e) {
                $('#ListBody').append(`<tr data-id="${e.ARID}" style="cursor: pointer">
                    <td class="pt-2 pb-1"><img src="${e.Picture}" class="img-fluid img-thumbnail" onerror="this.src='https://via.placeholder.com/600x200?text=ITicket'"></td>
                    <td class="pt-2 pb-1 font-weight-bold">${keyHighlight(e.Title, $('#fKeyword').val())}</td>
                    <td class="pt-2 pb-1">${keyHighlight(e.Author.split('@', 2)[0], $('#fKeyword').val())}<br><sup class="text-secondary">&nbsp;&nbsp;&nbsp;@${e.Author.split('@', 2)[1]}</sup></td>
                    <td class="pt-2 pb-1">${e.CategoryName}</td>
                    <td class="pt-2 pb-1">${e.Date.split(' ')[0]}&nbsp;&nbsp;&nbsp;${e.Date.split(' ')[1]}</td>
                    <td class="pt-2 pb-1">${e.ReportCount}</td>
                </tr>`)
            })
            let pagecurrent = parseInt($('#fPageCurrent').val())
            $(`#pageTop li`).not('.default').remove().end().removeClass('disabled')
            $(`#pageBottom li`).not('.default').remove().end().removeClass('disabled')
            if (pagecurrent === 1) {
                $(`#pageTop li`).first().addClass('disabled')
                $(`#pageBottom li`).first().addClass('disabled')
            }
            if (pagecurrent === maxpage) {
                $(`#pageTop li`).last().addClass('disabled')
                $(`#pageBottom li`).last().addClass('disabled')
            }
            let showpage = [1, 2, pagecurrent - 2, pagecurrent - 1, pagecurrent, pagecurrent + 1, pagecurrent + 2, maxpage - 1, maxpage]
            let pointer1 = $(`#pageTop li`).first()
            let pointer2 = $(`#pageBottom li`).first()
            let flag = false
            for (let i = 1; i <= maxpage; i++) {
                if (!showpage.includes(i) && flag) {
                    continue
                }
                if (!showpage.includes(i)) {
                    pointer1.after(
                        `<li class="page-item"><a class="page-link text-secondary customGotoNewPage" href="javascript:">…</a></li>`
                    )
                    pointer1 = pointer1.next()
                    pointer2.after(
                        `<li class="page-item"><a class="page-link text-secondary customGotoNewPage" href="javascript:">…</a></li>`
                    )
                    pointer2 = pointer2.next()
                    flag = true
                    continue
                }
                pointer1.after(
                    `<li class="page-item"><a data-id="${i}" class="page-link customGotoPage" href="javascript:" style="background-color: ${pagecurrent === i ? 'lightyellow' : 'transparent'}">${i}</a></li>`
                )
                pointer1 = pointer1.next()
                pointer2.after(
                    `<li class="page-item"><a data-id="${i}" class="page-link customGotoPage" href="javascript:" style="background-color: ${pagecurrent === i ? 'lightyellow' : 'transparent'}">${i}</a></li>`
                )
                pointer2 = pointer2.next()
                flag = false
            }
            let begin = ($('#fPageCurrent').val() - 1) * $('#fPageSize').val() + 1
            let ending = ($('#fPageCurrent').val() - 1) * $('#fPageSize').val() + data.length
            $('#pageMessage').text(begin <= ending ? `顯示第 ${begin} 筆到第 ${ending} 筆資料` : `沒有符合的資料`)
            if ($(window).scrollTop() > 225) {
                $(window).scrollTop(225)
            }
        }
    })
}

function theDetail(id) {
    const displayName = {
        'Email': 'Email',
        'Name': '姓名',
        'IDentityNumber': '身分證字號',
        'Passport': '護照',
        'NickName': '暱稱',
        'BirthDate': '生日',
        'Phone': '電話',
        'Point': '獎勵點數',
        'Address': '地址',
        'MemberRoleName': '角色權限',
        'Sex': '性別',
        'District': '城市',
        'SplitLine': '------------',
        'CompanyName': '公司名',
        'TaxIDNumber': '統編',
        'SellerHomePage': '商家網站主頁',
        'SellerPhone': '商家聯絡資訊',
        'SellerDiscription': '商家描述資訊',
        'fPass': '審核狀態'
    }
    $.ajax({
        url: '/BackEndMember/MemberDetail',
        type: 'post',
        data: { id: id },
        success: function (data) {
            let rows = ''
            for (let field in data) {
                if (field == 'SplitLine') {
                    rows += `<tr class="bg-dark"><td colspan="2"></td></tr>`
                    continue
                }
                if (field == 'fPass') {
                    let fPassClass
                    switch (data[field]) {
                        case '審核通過':
                            fPassClass = 'text-success'
                            break
                        case '審核不通過':
                            fPassClass = 'text-danger'
                            break
                        default:
                            fPassClass = 'text-warning'
                            break
                    }
                    rows += `<tr><th>${displayName[field]}</th><td class="${fPassClass}">${data[field] === null ? '' : data[field]}</td></tr>`
                    continue
                }
                rows += `<tr><th>${displayName[field]}</th><td>${data[field] === null ? '' : data[field]}</td></tr>`
            }
            let html = (`
                <div class="table-responsive text-body bg-white">
                    <table class="table table-bordered table-striped" style="width: 100%;">
                        ${rows}
                    <table>
                </div>
            `)
            $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
            $('#AjaxBoxBody').html(html)
            $('#AjaxBoxTitle').html('會員資料查詢')
            $('#OK').off('click').one('click', function () {
                $(this).prev().click()
            })
        }
    })
}

function thePrev() {
    let pagecurrent = parseInt($('#fPageCurrent').val())
    $('#fPageCurrent').val(Math.max(1, pagecurrent - 1))
    AjaxArticleList()
}

function theNext() {
    let pagecurrent = parseInt($('#fPageCurrent').val())
    $('#fPageCurrent').val(Math.min(pagecurrent + 1, maxpage))
    AjaxArticleList()
}

function keyHighlight(text, keyword) {
    const prefix = []
    const suffix = []
    for (let i = 0; i < text.length; i++) {
        if (text.toLowerCase().slice(i).search(keyword) === 0) {
            prefix.push(i)
            suffix.push(i + keyword.length)
        }
    }
    while (prefix.length) {
        let iS = suffix.pop()
        text = `${text.slice(0, iS)}</span>${text.slice(iS)}`
        let iP = prefix.pop()
        text = `${text.slice(0, iP)}<span style="background-color: springgreen">${text.slice(iP)}`
    }
    return text
}

function keyInput() {
    $('#fKeyword').val($('#searchbox').val().toLowerCase().trim())
    $('#fPageCurrent').val(1)
    $('#fPageSize').val(10)
    $('#pageAmount').prop('selectedIndex', 0)
    AjaxArticleList()
}