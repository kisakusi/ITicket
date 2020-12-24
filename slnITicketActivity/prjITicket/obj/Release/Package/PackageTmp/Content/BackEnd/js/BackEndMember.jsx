let CurrentTimer = 0
const CurrentPageDatas = []
const BackEndMemberListCurrentPage = []
const BackEndMemberListCollection = []

function AjaxMemberList() {
    CurrentTimer = new Date().getTime()
    $('#isHandle').removeClass('d-none')
    $.ajax({
        url: '/BackEndMember/MemberList',
        type: 'post',
        data: $('#ctrlForm').serialize() + `&CurrentTimer=${CurrentTimer}`,
        success: function (data) {
            if (CurrentTimer > data[0].CurrentTimer) {
                return
            }
            CurrentPageDatas.splice(0, CurrentPageDatas.length, ...data.slice(1))
            BackEndMemberListCurrentPage.splice(0, BackEndMemberListCurrentPage.length)
            for (let i of data.slice(1)) {
                BackEndMemberListCurrentPage.push(i.MemberID)
            }
            if (data[0].ChangePage != 0) {
                $('#fPageCurrent').val(data[0].ChangePage)
            }
            $('#TotalSearch').text(data[0].TotalSearch)
            $('#ctrlHint').text($('#fKeyword').val() == '' ? '' : `${$('#fSearchMode').val() === 'false' ? '完整關鍵字' : '模糊搜尋'}: ${$('#fKeyword').val()}`)
            if ($('#fSearchMode').val() === 'false') {
                $('#ctrlHint').addClass('text-success').removeClass('text-warning')
            } else {
                $('#ctrlHint').removeClass('text-success').addClass('text-warning')
            }
            ReactDOM.render(<MemberList data={data.slice(1)} />, document.querySelector('#listBody'))
            let maxpage = data[0].MaxPage
            let current = parseInt($('#fPageCurrent').val())
            ReactDOM.render(<Pagination page={current} max={maxpage} />, document.querySelector('#pageTop'))
            ReactDOM.render(<Pagination page={current} max={maxpage} />, document.querySelector('#pageBottom'))
            let begin = ($('#fPageCurrent').val() - 1) * $('#fPageSize').val() + 1
            let ending = ($('#fPageCurrent').val() - 1) * $('#fPageSize').val() + data.length - 1
            $('#pageMessage').text(begin <= ending ? `顯示第 ${begin} 筆到第 ${ending} 筆資料` : `沒有符合的資料`)
            $('#isHandle').addClass('d-none')
        }
    })
}

class MemberList extends React.Component {
    handleBoxClick = id => {
        let index = BackEndMemberListCollection.indexOf(id)
        if (index < 0) {
            BackEndMemberListCollection.push(id)
        } else {
            BackEndMemberListCollection.splice(index, 1)
        }
        ReactDOM.render(<MemberList data={this.props.data} />, document.querySelector('#listBody'))
    }
    handleIdClick = (id, isban) => {
        $('#AjaxBoxTag3-tab').addClass('d-none')
        $('#AjaxBoxTag5-tab').addClass('d-none')
        $('#BanTaskAction').removeClass('d-none')
        if (isban) {
            $('#UnBanTaskAction').removeClass('d-none')
        } else {
            $('#UnBanTaskAction').addClass('d-none')
        }
        $('#Confirm').addClass('d-none')
        $.ajax({
            url: '/BackEndMember/MemberDetail',
            type: 'post',
            data: {
                id: id
            },
            success: function (data) {
                ReactDOM.render(<MemberDetail tag={2} data={data} />, document.querySelector('#AjaxBoxTag2'))
                ReactDOM.render(<MemberDetail tag={3} data={data} />, document.querySelector('#AjaxBoxTag3'))
                ReactDOM.render(<MemberDetail tag={0} data={data} />, document.querySelector('#AjaxBoxTag4'))
                if (data.SellerID !== null) {
                    $('#AjaxBoxTag3-tab').removeClass('d-none')
                }
                $('#AjaxBoxTag2-tab').tab('show')
                $('#BanTaskAction').data('mid', id)
                $('#UnBanTaskAction').data('mid', id)
                $('#AjaxBox').modal({ show: true })
            }
        })
    }
    handleDataDownload = (id, email) => {
        $.ajax({
            url: '/BackEndMember/DataDownloadCheck',
            type: 'post',
            data: {
                id: id
            },
            success: function (data) {
                if (data === 'Success') {
                    swal('商家資料', `是否要下載 ${email} 商家的資料?`, 'warning', {
                        buttons: {
                            cancel: '取消',
                            confirm: '下載'
                        }
                    }).then(x => {
                        if (x) {
                            window.open(`/BackEndMember/DataDownload/${id}`)
                            swal('商家資料', '下載完成', 'success', {
                                button: false
                            })
                        }
                    })
                } else {
                    swal('商家資料', '商家並未上傳資料', 'error', {
                        button: false
                    })
                }
            }
        })
    }
    handleVeriClick = (id, email) => {
        swal('商家審核確認', `是否要通過 ${email} 商家的審核?`, 'warning', {
            dangerMode: true,
            buttons: {
                cancel: '取消',
                confirm: '駁回',
                pass: '通過'
            }
        }).then(x => {
            if (x === 'pass') {
                swal('商家審核進行中', '同步發送通知與站外寄信, 請稍後...', 'info', {
                    button: false,
                    closeOnClickOutside: false,
                    closeOnEsc: false,
                })
                $.ajax({
                    url: '/BackEndMember/VerifyAsync',
                    type: 'post',
                    data: {
                        id: id,
                        fPass: true,
                    },
                    timeout: 20000,
                    success: function (result) {
                        swal('商家審核完成', result, 'success', {
                            button: false
                        })
                        AjaxMemberList()
                    },
                    error: function (xmlhttprequest, textstatus, message) {
                        swal('商家審核終止', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                            button: false
                        })
                    }
                })
            } else if (x === true) {
                swal({
                    title: '請填寫不通過的理由',
                    content: 'input',
                    buttons: {
                        cancel: '取消',
                        confirm: '確認'
                    },
                    closeOnClickOutside: false,
                    closeOnEsc: false,
                }).then(message => {
                    if (message !== null && message.trim() !== '') {
                        swal('商家審核進行中', '同步發送通知與站外寄信, 請稍後...', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndMember/VerifyAsync',
                            type: 'post',
                            data: {
                                id: id,
                                fPass: false,
                                message: message
                            },
                            timeout: 20000,
                            success: function (result) {
                                swal('商家審核完成', result, 'success', {
                                    button: false
                                })
                                AjaxMemberList()
                            },
                            error: function (xmlhttprequest, textstatus, message) {
                                swal('商家審核終止', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                    button: false
                                })
                            }
                        })
                    }
                })
            }
        })
    }
    handleBanClick = id => {
        $('#AjaxBoxTag3-tab').addClass('d-none')
        $('#AjaxBoxTag5-tab').removeClass('d-none')
        $('#BanTaskAction').addClass('d-none')
        $('#UnBanTaskAction').addClass('d-none')
        $.ajax({
            url: '/BackEndMember/MemberDetail',
            type: 'post',
            data: {
                id: id
            },
            success: function (data) {
                ReactDOM.render(<MemberDetail tag={2} data={data} />, document.querySelector('#AjaxBoxTag2'))
                ReactDOM.render(<MemberDetail tag={3} data={data} />, document.querySelector('#AjaxBoxTag3'))
                ReactDOM.render(<MemberDetail tag={0} data={data} />, document.querySelector('#AjaxBoxTag4'))
                ReactDOM.render(<BanDetail tag={'ban'} mid={id} xid={0} />, document.querySelector('#AjaxBoxTag5'))
                if (data.SellerID !== null) {
                    $('#AjaxBoxTag3-tab').removeClass('d-none')
                }
                $('#AjaxBoxTag5-tab').tab('show')
                $('#AjaxBox').modal({ show: true })
                $('#Confirm').removeClass('d-none').off('click').on('click', function () {
                    if ($('#BanTaskMessage').val().trim() === '') {
                        swal('無法執行', '請填寫系統通知', 'error', {
                            button: false
                        })
                    } else {
                        $.ajax({
                            url: '/BackEndMember/MemberEmailCollection',
                            type: 'post',
                            data: {
                                BackEndMemberListCollection: [id]
                            },
                            success: function (data) {
                                swal('停權會員確認', `是否要停權 ${data[0].email}？`, 'warning', {
                                    buttons: {
                                        cancel: '取消',
                                        confirm: '確定'
                                    }
                                }).then(x => {
                                    if (x) {
                                        swal('停權會員進行中', '同步發送通知與站外寄信, 請稍後...', 'info', {
                                            button: false,
                                            closeOnClickOutside: false,
                                            closeOnEsc: false,
                                        })
                                        $.ajax({
                                            url: '/BackEndMember/BanMemberAsync',
                                            type: 'post',
                                            data: $('#BanTask').serialize(),
                                            success: function (result) {
                                                swal('停權會員成功', result, 'success', {
                                                    button: false
                                                })
                                                AjaxMemberList()
                                            },
                                            timeout: 20000,
                                            error: function (xmlhttprequest, textstatus, message) {
                                                swal('停權會員終止', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                                    button: false
                                                })
                                            }
                                        })
                                        $('#Cancel').click()
                                    }
                                })
                            }
                        })
                    }
                })
            }
        })
    }
    handleUnBanClick = id => {
        $.ajax({
            url: '/BackEndMember/MemberEmailCollection',
            type: 'post',
            data: {
                BackEndMemberListCollection: [id]
            },
            success: function (data) {
                swal('解除停權確認', `是否要解除使用者尚未期滿的所有停權處分？\n　　➢ ${data[0].email}`, 'warning', {
                    buttons: {
                        cancel: '取消',
                        confirm: '確定'
                    }
                }).then(x => {
                    if (x) {
                        swal('解除停權進行中', '同步發送通知與站外寄信, 請稍後...', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndMember/UnBanMemberAsync',
                            type: 'post',
                            data: {
                                id: id
                            },
                            success: function (result) {
                                swal('解除停權成功', result, 'success', {
                                    button: false
                                })
                                AjaxMemberList()
                            },
                            timeout: 20000,
                            error: function (xmlhttprequest, textstatus, message) {
                                swal('解除停權終止', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                    button: false
                                })
                            }
                        })
                    }
                })
            }
        })
    }
    handleReasonClick = reason => {
        swal('停權原因', reason, 'info')
    }
    render() {
        let fKeyword = document.querySelector('#fKeyword').value
        return (
            <React.Fragment>
                {this.props.data.map(e =>
                    <tr className={e.Reasons.length ? 'text-danger' : 'text-body'}
                        style={{ backgroundColor: BackEndMemberListCollection.includes(e.MemberID) ? 'lightyellow' : 'white' }}
                    >
                        <td className="pt-2 pb-1 text-body" style={{ cursor: 'pointer' }}
                            onClick={(x) => this.handleBoxClick(e.MemberID)}
                        >
                            {
                                BackEndMemberListCollection.includes(e.MemberID) &&
                                <span><i className="far fa-check-square"></i></span>
                            }
                            {
                                !BackEndMemberListCollection.includes(e.MemberID) &&
                                <span><i className="far fa-square"></i></span>
                            }
                        </td>
                        <td className="pt-2 pb-1">
                            <a href="javascript:"
                                onClick={() => this.handleIdClick(e.MemberID, e.Reasons.length != 0)}
                                dangerouslySetInnerHTML={{ __html: keyHighlight(e.MemberEmail.split('@', 2)[0], fKeyword, e.SearchMode) }}></a>
                            <br />
                            <sup className="text-secondary">&nbsp;&nbsp;&nbsp;@{e.MemberEmail.split('@', 2)[1]}</sup>
                        </td>
                        <td className="pt-2 pb-1" dangerouslySetInnerHTML={{ __html: keyHighlight(e.MemberNickName, fKeyword, e.SearchMode) }}></td>
                        <td className="pt-2 pb-1" dangerouslySetInnerHTML={{ __html: keyHighlight(e.MemberName, fKeyword, e.SearchMode) }}></td>
                        <td className="pt-2 pb-1" dangerouslySetInnerHTML={{ __html: keyHighlight(e.MemberPhone || '', fKeyword, e.SearchMode) }}></td>
                        <td className="pt-2 pb-1">{e.MemberRoleName}
                            {
                                e.Reasons.length != 0 &&
                                <span className="badge badge-danger ml-1">停權中</span>
                            }
                            {
                                e.MemberRoleId == 3 && e.fPass == "尚未審核" &&
                                <span className="badge badge-warning text-white ml-1">未審核</span>
                            }
                            {
                                e.MemberRoleId == 3 && e.fPass == "審核通過" &&
                                <span className="badge badge-success text-white ml-1">通過</span>
                            }
                            {
                                e.MemberRoleId == 3 && e.fPass == "審核不通過" &&
                                <span className="badge badge-secondary text-white ml-1">不通過</span>
                            }
                        </td>
                        <td className="pt-2 pb-1">
                            {
                                e.MemberRoleId == 3 &&
                                <button className="btn btn-info btn-sm text-white mr-2" onClick={() => this.handleDataDownload(e.MemberID, e.MemberEmail)}>資料</button>
                            }
                            {
                                e.MemberRoleId == 3 && e.fPass == "尚未審核" &&
                                <button className="btn btn-warning btn-sm text-white mr-2" onClick={() => this.handleVeriClick(e.MemberID, e.MemberEmail)}>審核</button>
                            }
                            {
                                e.MemberRoleId != 4 && e.Reasons.length == 0 &&
                                <button className="btn btn-danger btn-sm mr-2" onClick={() => this.handleBanClick(e.MemberID)}>停權</button>
                            }
                            {
                                e.MemberRoleId != 4 && e.Reasons.length != 0 &&
                                <React.Fragment>
                                    <button className="btn btn-secondary btn-sm mr-2" onClick={() => this.handleUnBanClick(e.MemberID)}>解除</button>
                                    <span>{e.EndTimes[0]} <a href="javascript:" onClick={() => this.handleReasonClick(e.Reasons[0])}>停權原因</a></span>
                                </React.Fragment>
                            }
                        </td>
                    </tr>
                )}
            </React.Fragment>
        )
    }
}

class SendMessageList extends React.Component {
    render() {
        return (
            <React.Fragment>
                {this.props.data.map(e => 
                    <p className="text-success"><i class="far fa-bell"></i> {e.email}</p>
                )}
            </React.Fragment>
        )
    }
}

$(function () {
    // DOMContentLoaded
    AjaxMemberList()
    $('#ctrlBtn4').addClass('btn-outline-success')

    // Basic - PageSize Change
    $('#pageAmount').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val($('#pageAmount').val())
        AjaxMemberList()
    })

    // Basic - Keyword Change
    $('#searchbox').on('input', keyInput).on('compositionstart', function () {
        $(this).off('input')
    }).on('compositionend', function () {
        keyInput()
        $(this).on('input', keyInput)
    })

    // Basic - Sort Change
    $('#listHead1, #listHead2, #listHead3, #listHead4').on('click', function () {
        let i = parseInt(this.id.slice(-1))
        $('#fPageCurrent').val(1)
        $('#fSort').val($('#fSort').val() === `${i}a` ? `${i}d` : $('#fSort').val() === `${i}d` ? '0' : `${i}a`)
        MemberSortFont($('#fSort').val())
        AjaxMemberList()
    })

    // Basic - RoleId Change
    $('#ctrlBtn4, #ctrlBtn3, #ctrlBtn2, #ctrlBtn1, #ctrlBtn0').on('mouseenter', function () {
        let i = parseInt(this.id.slice(-1))
        $(this).addClass(i === 4 ? 'btn-outline-success' : i ? 'btn-outline-warning' : 'btn-outline-danger')
    }).on('mouseleave', function () {
        let i = parseInt(this.id.slice(-1))
        if (parseInt($('#fRoleId').val()) !== i) {
            $(this).removeClass(i === 4 ? 'btn-outline-success' : i ? 'btn-outline-warning' : 'btn-outline-danger')
        }
    }).on('click', function () {
        let i = parseInt(this.id.slice(-1))
        if (parseInt($('#fRoleId').val()) === i) {
            return
        }
        $(this).css({
            'outline': 'none !important',
            'box-shadow': 'none'
        }).addClass(i === 4 ? 'btn-outline-success' : i ? 'btn-outline-warning' : 'btn-outline-danger')
            .siblings().removeClass('btn-outline-success').removeClass('btn-outline-warning').removeClass('btn-outline-danger')
        $('#fRoleId').val(i)
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        $('#fSort').val(0)
        MemberSortFont($('#fSort').val())
        AjaxMemberList()
    })

    $('#ctrlVeri').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        $('#fVeri').val($(this).prop('checked'))
        AjaxMemberList()
    })

    // Basic Reset
    $('#ctrlReset').css({
        'outline': 'none !important',
        'box-shadow': 'none'
    }).on('click', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        $('#searchbox').val('')
        $('#fKeyword').val('')
        $('#fSort').val(0)
        MemberSortFont($('#fSort').val())
        $('#fRoleId').val(4)
        $('#ctrlBtn4').addClass('btn-outline-success')
        $('#ctrlBtn3, #ctrlBtn2, #ctrlBtn1').removeClass('btn-outline-warning')
        $('#ctrlBtn0').removeClass('btn-outline-danger')
        $('#fVeri').val(false)
        $('#ctrlVeri').prop('checked', false)
        $('#fSearchMode').val(false)
        $('#ctrlSearchMode').prop('checked', false)
        AjaxMemberList()
    })

    // Pro - SearchMode Change
    $('#ctrlSearchMode').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        $('#fSort').val(0)
        MemberSortFont($('#fSort').val())
        $('#fSearchMode').val($(this).prop('checked'))
        AjaxMemberList()
    })

    // AnotherAction - SelectAll
    $('#listHead0').on('click', function () {
        if (BackEndMemberListCurrentPage.every(x => BackEndMemberListCollection.includes(x))) {
            for (let i of BackEndMemberListCurrentPage) {
                let index = BackEndMemberListCollection.indexOf(i)
                BackEndMemberListCollection.splice(index, 1)
            }
        } else {
            for (let i of BackEndMemberListCurrentPage) {
                let index = BackEndMemberListCollection.indexOf(i)
                if (index < 0) {
                    BackEndMemberListCollection.push(i)
                }
            }
        }
        ReactDOM.render(<MemberList data={CurrentPageDatas} />, document.querySelector('#listBody'))
    })

    // AnotherAction - SendMessageAsync
    $('#ctrlBtn5').on('click', function () {
        if (!BackEndMemberListCollection.length) {
            swal('無法執行', '沒有選擇任何會員', 'error', {
                button: false
            })
        } else {
            $.ajax({
                url: '/BackEndMember/MemberEmailCollection',
                type: 'post',
                data: {
                    BackEndMemberListCollection: BackEndMemberListCollection
                }, 
                success: function (data) {
                    ReactDOM.render(<SendMessageList data={data} />, document.querySelector('#MsgBoxTarget'))
                    $('#MsgBox').modal({ show: true })
                    $('#MsgConfirm').on('click', function () {
                        if ($('#MsgBoxTextarea').val().trim() === '') {
                            swal('無法執行', '請填寫系統通知', 'error', {
                                button: false
                            })
                        } else {
                            swal('系統通知發送確認', `是否要發送系統通知給 ${data.length} 位會員？`, 'warning', {
                                buttons: {
                                    cancel: '取消',
                                    confirm: '確定'
                                },
                                closeOnClickOutside: false,
                                closeOnEsc: false,
                            }).then(x => {
                                if (x) {
                                    swal('系統通知發送中', '同步發送通知與站外寄信, 請稍後...', 'info', {
                                        button: false,
                                        closeOnClickOutside: false,
                                        closeOnEsc: false,
                                    })
                                    $.ajax({
                                        url: '/BackEndMember/SendMessageAsync',
                                        type: 'post',
                                        data: {
                                            BackEndMemberListCollection: BackEndMemberListCollection,
                                            message: $('#MsgBoxTextarea').val().trim()
                                        },
                                        success: function (result) {
                                            swal('系統通知發送成功', result, 'success', {
                                                button: false
                                            })
                                            $('#MsgBoxTextarea').val('')
                                            BackEndMemberListCollection.splice(0, BackEndMemberListCollection.length)
                                            ReactDOM.render(<MemberList data={CurrentPageDatas} />, document.querySelector('#listBody'))
                                        },
                                        timeout: 20000,
                                        error: function (xmlhttprequest, textstatus, message) {
                                            swal('系統通知發送終止', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                                button: false
                                            })
                                        }
                                    })
                                    $('#MsgCancel').click()
                                }
                            })
                        }
                    })
                }
            })
        }
    })

    // AnotherAction - MemberDownload / SellerDownload
    $('#ctrlDownload').on('click', function () {
        swal('下載資料確認', '是否要下載資料', 'warning', {
            buttons: {
                cancel: '取消',
                member: '下載所有會員資料',
                seller: '下載所有商家資料',
                confirm: false
            }
        }).then(x => {
            if (x === 'member') {
                window.open('/BackEndMember/MemberDownload')
                swal('下載資料完成', '成功下載所有會員資料', 'success', {
                    button: false,
                })
            } else if (x === 'seller') {
                window.open('/BackEndMember/SellerDownload')
                swal('下載資料完成', '成功下載所有商家資料', 'success', {
                    button: false,
                })
            }
        })
    })
    
    // AjaxBox - BanTaskAction
    $('#BanTaskAction').on('click', function () {
        $('#BanTaskAction').addClass('d-none')
        $('#UnBanTaskAction').addClass('d-none')
        $('#AjaxBoxTag5-tab').removeClass('d-none')
        ReactDOM.render(
            <BanDetail tag={'ban'} mid={$('#BanTaskAction').data('mid')} xid={0} />,
            document.querySelector('#AjaxBoxTag5')
        )
        $('#AjaxBoxTag5-tab').tab('show')
        $('#Confirm').removeClass('d-none').off('click').on('click', function () {
            if ($('#BanTaskMessage').val().trim() === '') {
                swal('無法執行', '請填寫系統通知', 'error', {
                    button: false
                })
            } else {
                $.ajax({
                    url: '/BackEndMember/MemberEmailCollection',
                    type: 'post',
                    data: {
                        BackEndMemberListCollection: [$('#BanTaskAction').data('mid')]
                    }, 
                    success: function (data) {
                        swal('停權會員確認', `是否要停權 ${data[0].email}？`, 'warning', {
                            buttons: {
                                cancel: '取消',
                                confirm: '確定'
                            }
                        }).then(x => {
                            if (x) {
                                swal('停權會員進行中', '同步發送通知與站外寄信, 請稍後...', 'info', {
                                    button: false,
                                    closeOnClickOutside: false,
                                    closeOnEsc: false,
                                })
                                $.ajax({
                                    url: '/BackEndMember/BanMemberAsync',
                                    type: 'post',
                                    data: $('#BanTask').serialize(),
                                    success: function (result) {
                                        swal('停權會員成功', result, 'success', {
                                            button: false
                                        })
                                        AjaxMemberList()
                                    },
                                    timeout: 20000,
                                    error: function (xmlhttprequest, textstatus, message) {
                                        swal('停權會員終止', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                            button: false
                                        })
                                    }
                                })
                                $('#Cancel').click()
                            }
                        })
                    }
                })
            }
        })
    })
    
    // AjaxBox - UnBanTaskAction
    $('#UnBanTaskAction').on('click', function () {
        $.ajax({
            url: '/BackEndMember/MemberEmailCollection',
            type: 'post',
            data: {
                BackEndMemberListCollection: [$('#UnBanTaskAction').data('mid')]
            }, 
            success: function (data) {
                swal('解除停權確認', `是否要解除使用者尚未期滿的所有停權處分？\n　　➢ ${data[0].email}`, 'warning', {
                    buttons: {
                        cancel: '取消',
                        confirm: '確定'
                    }
                }).then(x => {
                    if (x) {
                        swal('解除停權進行中', '同步發送通知與站外寄信, 請稍後...', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndMember/UnBanMemberAsync',
                            type: 'post',
                            data: {
                                id: $('#UnBanTaskAction').data('mid')
                            },
                            success: function (result) {
                                swal('解除停權成功', result, 'success', {
                                    button: false
                                })
                                AjaxMemberList()
                            },
                            timeout: 20000,
                            error: function (xmlhttprequest, textstatus, message) {
                                swal('解除停權終止', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                    button: false
                                })
                            }
                        })
                        $('#Cancel').click()
                    }
                })
            }
        })
    })    
})
