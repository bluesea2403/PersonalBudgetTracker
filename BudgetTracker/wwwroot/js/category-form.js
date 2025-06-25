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