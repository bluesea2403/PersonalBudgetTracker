
console.log('✅ site.js loaded');
// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function showInPopup(url, title) {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#transactionModal .modal-body').html(res);
            $('#transactionModal .modal-title').html(title);
            $('#transactionModal').modal('show');
        }
    });
}

function jQueryAjaxPost(form) {
    try {
        $.ajax({
            type: 'POST',
            url: $(form).attr('action'),
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#transactionModal').modal('hide');
                    location.reload(); // Cách đơn giản nhất là tải lại trang
                } else {
                    $('#transactionModal .modal-body').html(res.html);
                }
            },
            error: function (err) {
                console.log(err);
            }
        });
    } catch (e) {
        console.log(e);
    }
    // return false để ngăn form submit theo cách truyền thống
    return false;
}
function confirmDelete(id) {
    Swal.fire({
        title: 'Bạn chắc chắn muốn xóa?',
        text: "Hành động này không thể được hoàn tác!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Vâng, xóa nó!',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            // Nếu người dùng nhấn "Vâng, xóa nó!"
            $.ajax({
                type: 'POST',
                url: '/Transactions/Delete/' + id,
                // Thêm token chống giả mạo vào header của request
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (res) {
                    if (res.success) {
                        Swal.fire(
                            'Đã xóa!',
                            res.message,
                            'success'
                        ).then(() => {
                            location.reload(); // Tải lại trang để cập nhật danh sách
                        });
                    } else {
                        Swal.fire('Lỗi!', res.message, 'error');
                    }
                },
                error: function (err) {
                    Swal.fire('Lỗi!', 'Đã có lỗi xảy ra trong quá trình xóa.', 'error');
                }
            });
        }
    });
}
function selectIcon(element, iconClass) {
    // Cập nhật giá trị cho ô input ẩn
    document.getElementById('Icon').value = iconClass;

    // Xóa lớp 'selected' khỏi tất cả các item khác
    document.querySelectorAll('#icon-picker .icon-item').forEach(item => {
        item.classList.remove('selected');
    });

    // Thêm lớp 'selected' cho item vừa được nhấn để tạo hiệu ứng
    element.classList.add('selected');
}
function uploadInvoice(input) {
    if (input.files && input.files[0]) {
        let formData = new FormData();
        formData.append('invoiceFile', input.files[0]);

        Swal.fire({
            title: 'Đang xử lý hóa đơn...',
            text: 'Vui lòng chờ trong giây lát.',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        fetch('/api/Ocr/UploadInvoice', {
            method: 'POST',
            body: formData
        })
            .then(response => {
                Swal.close();
                if (!response.ok) {
                    throw new Error('Server response was not ok. Status: ' + response.status);
                }
                return response.json();
            })
            .then(result => {
                // ĐÂY LÀ DÒNG CODE ĐÚNG, SỬ DỤNG DẤU BACKTICK (`) VÀ CÚ PHÁP ${...}
                let url = `/Transactions/Create?amount=${result.amount}&description=${encodeURIComponent(result.description)}`;
                showInPopup(url, 'Xác nhận Giao dịch từ Hóa đơn');
            })
            .catch(error => {
                Swal.fire('Lỗi!', 'Không thể xử lý hóa đơn. Vui lòng kiểm tra lại đường dẫn Tesseract trong code.', 'error');
                console.error('Error:', error);
            });
    }
}
$(document).ready(function () {
    // ========== DARK MODE ==========
    const themeToggle = $('#theme-toggle');
    const htmlEl = $('html');

    function updateIcon(isDarkMode) {
        themeToggle.html(isDarkMode ? '<i class="fas fa-sun"></i>' : '<i class="fas fa-moon"></i>');
    }

    let savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        htmlEl.addClass('dark-mode');
        updateIcon(true);
    } else {
        htmlEl.removeClass('dark-mode');
        updateIcon(false);
    }

    themeToggle.on('click', function () {
        if (htmlEl.hasClass('dark-mode')) {
            htmlEl.removeClass('dark-mode');
            localStorage.setItem('theme', 'light');
            updateIcon(false);
        } else {
            htmlEl.addClass('dark-mode');
            localStorage.setItem('theme', 'dark');
            updateIcon(true);
        }
    });

    // ========== CHATBOT ==========
    const chatBubble = $('#chat-bubble');
    const chatWidget = $('#chat-widget');
    const chatCloseBtn = $('#chat-close-btn');
    const chatMessages = $('#chat-messages');
    const chatInput = $('#chat-input');
    const chatSendBtn = $('#chat-send-btn');

    chatBubble.on('click', function () {
        console.log('✅ Chat bubble clicked');
        chatWidget.removeClass('d-none');
        chatBubble.addClass('d-none');
    });

    chatCloseBtn.on('click', function () {
        chatWidget.addClass('d-none');
        chatBubble.removeClass('d-none');
    });

    function sendMessage() {
        const messageText = chatInput.val().trim();
        if (messageText === '') return;

        chatMessages.append(`<div class="chat-message user-message"><p>${messageText}</p></div>`);
        chatInput.val('');
        chatMessages.scrollTop(chatMessages[0].scrollHeight);

        chatMessages.append(`
            <div class="chat-message bot-message" id="typing-indicator">
                <p class="typing-indicator"><span></span><span></span><span></span></p>
            </div>
        `);
        chatMessages.scrollTop(chatMessages[0].scrollHeight);

        fetch('/api/Chatbot/SendMessage', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ text: messageText })
        })
            .then(response => response.json())
            .then(data => {
                $('#typing-indicator').remove();
                chatMessages.append(`<div class="chat-message bot-message"><p>${data.reply}</p></div>`);
                chatMessages.scrollTop(chatMessages[0].scrollHeight);
            })
            .catch(error => {
                $('#typing-indicator').remove();
                console.error('Error with chatbot:', error);
                chatMessages.append(`<div class="chat-message bot-message"><p>Rất tiếc, đã có lỗi xảy ra.</p></div>`);
                chatMessages.scrollTop(chatMessages[0].scrollHeight);
            });
    }

    chatSendBtn.on('click', sendMessage);
    chatInput.on('keypress', function (e) {
        if (e.which === 13) sendMessage();
    });

    // ========== XÓA HÀNG LOẠT ==========
    const selectAllCheckbox = $('#select-all-checkbox');
    const transactionCheckboxes = $('.transaction-checkbox');
    const deleteSelectedBtn = $('#delete-selected-btn');

    function toggleDeleteButton() {
        const checkedCount = $('.transaction-checkbox:checked').length;
        deleteSelectedBtn.toggle(checkedCount > 0);
    }

    selectAllCheckbox.on('change', function () {
        transactionCheckboxes.prop('checked', this.checked);
        toggleDeleteButton();
    });

    transactionCheckboxes.on('change', function () {
        if (!this.checked) selectAllCheckbox.prop('checked', false);
        toggleDeleteButton();
    });

    deleteSelectedBtn.on('click', function () {
        const selectedIds = [];
        $('.transaction-checkbox:checked').each(function () {
            selectedIds.push($(this).val());
        });

        if (selectedIds.length > 0) {
            Swal.fire({
                title: `Bạn chắc chắn muốn xóa ${selectedIds.length} mục?`,
                text: "Hành động này không thể được hoàn tác!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Vâng, xóa nó!',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    fetch('/Transactions/DeleteMultiple', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        body: JSON.stringify(selectedIds)
                    })
                        .then(response => response.json())
                        .then(data => {
                            if (data.success) {
                                Swal.fire('Đã xóa!', data.message, 'success').then(() => location.reload());
                            } else {
                                Swal.fire('Lỗi!', data.message, 'error');
                            }
                        })
                        .catch(error => Swal.fire('Lỗi!', 'Có lỗi xảy ra trong quá trình xóa.', 'error'));
                }
            });
        }
    });

    // ========== TOOLTIP ==========
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
