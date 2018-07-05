using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    public class City
    {
        public City(string name, string[] districts)
        {
            Name = name;
            Districts = districts;
        }
        public string Name { get; set; }
        public string[] Districts { get; set; }
    }
    public class Country
    {
        public Country(string name, City[] cities)
        {
            Name = name;
            Cities = cities;
        }
        public string Name { get; set; }
        public City[] Cities { get; set; }
        public static Country VietNam
        {
            get
            {
                return new Country(
                    "Việt Nam",
                    new City[]
                    {
                        new City("An Giang", new string[] {"An Phú","Châu Đốc","Châu Phú","Châu Thành","Chợ Mới","Long Xuyên","Phú Tân","Tân Châu","Thoại Sơn","Tịnh Biên","Tri Tôn"}),
                        new City("Bà Rịa - Vũng Tàu", new string[] {"Côn Đảo","Đất Đỏ","Tân Thành","Vũng Tàu","Xuyên Mộc","Bà Rịa","Châu Đức","Long Điền"}),
                        new City("Bắc Giang", new string[] {"Bắc Giang","Hiệp Hòa","Lạng Giang","Lục Nam","Lục Ngạn","Sơn Động","Tân Yên","Việt Yên","Yên Dũng","Yên Thế"}),
                        new City("Bắc Kạn", new string[] {"Ba Bể","Bạch Thông","Bắc Kạn","Chợ Đồn","Chợ Mới","Na Rì","Ngân Sơn","Pác Nặm"}),
                        new City("Bạc Liêu", new string[] {"Bạc Liêu","Đông Hải","Giá Rai","Hoà Bình","Hồng Dân","Phước Long","Vĩnh Lợi"}),
                        new City("Bắc Ninh", new string[] {"Bắc Ninh","Gia Bình","Lương Tài","Quế Võ","Thuận Thành","Tiên Du","Từ Sơn","Yên Phong"}),
                        new City("Bến Tre", new string[] {"Ba Tri","Bến Tre","Bình Đại","Châu Thành","Chợ Lách","Giồng Trôm","Mỏ Cày Bắc","Mỏ Cày Nam","Thạnh Phú"}),
                        new City("Bình Định", new string[] {"An Lão","An Nhơn","Hoài Ân","Hoài Nhơn","Phù Cát","Phù Mỹ","Quy Nhơn","Tây Sơn","Tuy Phước","Vân Canh","Vĩnh Thạnh"}),
                        new City("Bình Dương", new string[] {"Bàu Bàng","Bắc Tân Uyên","Bến Cát","Dầu Tiếng","Dĩ An","Phú Giáo","Tân Uyên","Thủ Dầu Một","Thuận An"}),
                        new City("Bình Phước", new string[] {"Bình Long","Bù Đăng","Bù Đốp","Bù Gia Mập","Chơn Thành","Đồng Phú","Đồng Xoài","Hớn Quản","Lộc Ninh","Phú Riềng","Phước Long"}),
                        new City("Bình Thuận", new string[] {"Bắc Bình","Đức Linh","Hàm Tân","Hàm Thuận Bắc","Hàm Thuận Nam","La Gi","Phan Thiết","Phú Quý","Tánh Linh","Tuy Phong"}),
                        new City("Cà Mau", new string[] {"Cà Mau","Cái Nước","Đầm Dơi","Năm Căn","Ngọc Hiển","Phú Tân","Thới Bình","Trần Văn Thời","U Minh"}),
                        new City("Cần Thơ", new string[] {"Bình Thủy","Cái Răng","Cờ Đỏ","Ninh Kiều","Ô Môn","Phong Điền","Thốt Nốt","Thới Lai","Vĩnh Thạnh"}),
                        new City("Cao Bằng", new string[] {"Bảo Lạc","Bảo Lâm","Cao Bằng","Hà Quảng","Hạ Lang","Hòa An","Nguyên Bình","Phục Hòa","Quảng Uyên","Thạch An","Thông Nông","Trà Lĩnh","Trùng Khánh"}),
                        new City("Đà Nẵng", new string[] {"Cẩm Lệ","Hải Châu","Hòa Vang","Hoàng Sa","Liên Chiểu","Ngũ Hành Sơn","Sơn Trà","Thanh Khê"}),
                        new City("Đắk Lắk", new string[] {"Buôn Đôn","Buôn Hồ","Buôn Ma Thuột","Cư Kuin","Cư M'gar","Ea H'leo","Ea Kar","Ea Súp","Krông Ana","Krông Bông","Krông Búk","Krông Năng","Krông Pắk","Lắk","M'Đrăk"}),
                        new City("Đăk Nông", new string[] {"Gia Nghĩa","Tuy Đức","Cư Jút","Đắk Glong","Đắk Mil","Đắk R'lấp","Đăk Song","Krông Nô"}),
                        new City("Điện Biên", new string[] {"Điện Biên","Điện Biên Đông","Điện Biên Phủ","Mường Ảng","Mường Chà","Mường Lay","Mường Nhé","Nậm Pồ","Tủa Chùa","Tuần Giáo"}),
                        new City("Đồng Nai", new string[] {"Biên Hòa","Cẩm Mỹ","Định Quán","Long Khánh","Long Thành","Nhơn Trạch","Tân Phú","Thống Nhất","Trảng Bom","Vĩnh Cửu","Xuân Lộc"}),
                        new City("Đồng Tháp", new string[] {"Cao Lãnh","Cao Lãnh","Châu Thành","Hồng Ngự","Hồng Ngự","Lai Vung","Lấp Vò","Sa Đéc","Tam Nông","Tân Hồng","Thanh Bình","Tháp Mười"}),
                        new City("Gia Lai", new string[] {"An Khê","Ayun Pa","Chư Păh","Chư Prông","Chư Pưh","Chư Sê","Đắk Đoa","Đak Pơ","Đức Cơ","Ia Grai","Ia Pa","KBang","Kông Chro","Krông Pa","Mang Yang","Phú Thiện","Pleiku"}),
                        new City("Hà Giang", new string[] {"Bắc Mê","Bắc Quang","Đồng Văn","Hà Giang","Hoàng Su Phì","Mèo Vạc","Quản Bạ","Quang Bình","Vị Xuyên","Xín Mần","Yên Minh"}),
                        new City("Hà Nam", new string[] {"Bình Lục","Duy Tiên","Kim Bảng","Lý Nhân","Phủ Lý","Thanh Liêm"}),
                        new City("Hà Nội", new string[] {"Ba Đình","Ba Vì","Bắc Từ Liêm","Cầu Giấy","Chương Mỹ","Đan Phượng","Đông Anh","Đống Đa","Gia Lâm","Hà Đông","Hai Bà Trưng","Hoài Đức","Hoàn Kiếm","Hoàng Mai","Long Biên","Mê Linh","Mỹ Đức","Nam Từ Liêm","Phú Xuyên","Phúc Thọ","Quốc Oai","Sóc Sơn","Sơn Tây","Tây Hồ","Thạch Thất","Thanh Oai","Thanh Trì","Thanh Xuân","Thường Tín","Ứng Hòa"}),
                        new City("Hà Tĩnh", new string[] {"Can Lộc","Cẩm Xuyên","Đức Thọ","Hà Tĩnh","Hồng Lĩnh","Hương Khê","Hương Sơn","Kỳ Anh","Kỳ Anh","Lộc Hà","Nghi Xuân","Thạch Hà","Vũ Quang"}),
                        new City("Hải Dương", new string[] {"Bình Giang","Cẩm Giàng","Chí Linh","Gia Lộc","Hải Dương","Kim Thành","Kinh Môn","Nam Sách","Ninh Giang","Thanh Hà","Thanh Miện","Tứ Kỳ"}),
                        new City("Hải Phòng", new string[] {"An Dương","An Lão","Bạch Long Vĩ","Cát Hải","Dương Kinh","Đồ Sơn","Hải An","Hồng Bàng","Kiến An","Kiến Thụy","Lê Chân","Ngô Quyền","Thuỷ Nguyên","Tiên Lãng","Vĩnh Bảo"}),
                        new City("Hậu Giang", new string[] {"Châu Thành","Châu Thành A","Long Mỹ","Long Mỹ","Ngã Bảy","Phụng Hiệp","Vị Thanh","Vị Thủy"}),
                        new City("Hoà Bình", new string[] {"Cao Phong","Đà Bắc","Hoà Bình","Kim Bôi","Kỳ Sơn","Lạc Sơn","Lạc Thủy","Lương Sơn","Mai Châu","Tân Lạc","Yên Thủy"}),
                        new City("Hưng Yên", new string[] {"Ân Thi","Hưng Yên","Khoái Châu","Kim Động","Mỹ Hào","Phù Cừ","Tiên Lữ","Văn Giang","Văn Lâm","Yên Mỹ"}),
                        new City("Khánh Hòa", new string[] {"Cam Lâm","Cam Ranh","Diên Khánh","Khánh Sơn","Khánh Vĩnh","Nha Trang","Ninh Hòa","Trường Sa","Vạn Ninh"}),
                        new City("Kiên Giang", new string[] {"An Biên","An Minh","Châu Thành","Giang Thành","Giồng Riềng","Gò Quao","Hà Tiên","Hòn Đất","Kiên Hải","Kiên Lương","Phú Quốc","Rạch Giá","Tân Hiệp","U Minh Thượng","Vĩnh Thuận"}),
                        new City("Kon Tum", new string[] {"Đắk Glei","Đắk Hà","Đăk Tô","Ia H'Drai","Kon Plông","Kon Rẫy","Kon Tum","Ngọc Hồi","Sa Thầy","Tu Mơ Rông"}),
                        new City("Lai Châu", new string[] {"Lai Châu","Mường Tè","Nậm Nhùn","Phong Thổ","Sìn Hồ","Tam Đường","Tân Uyên","Than Uyên"}),
                        new City("Lâm Đồng", new string[] {"Bảo Lâm","Bảo Lộc","Cát Tiên","Di Linh","Đà Lạt","Đạ Huoai","Đạ Tẻh","Đam Rông","Đơn Dương","Đức Trọng","Lạc Dương","Lâm Hà"}),
                        new City("Lạng Sơn", new string[] {"Bắc Sơn","Bình Gia","Cao Lộc","Chi Lăng","Đình Lập","Hữu Lũng","Lạng Sơn","Lộc Bình","Tràng Định","Vãn Lãng","Văn Quan"}),
                        new City("Lào Cai", new string[] {"Bảo Thắng","Bảo Yên","Bát Xát","Bắc Hà","Lào Cai","Mường Khương","Sa Pa","Si Ma Cai","Văn Bàn"}),
                        new City("Long An", new string[] {"Bến Lức","Cần Đước","Cần Giuộc","Châu Thành","Đức Hòa","Đức Huệ","Kiến Tường","Mộc Hóa","Tân An","Tân Hưng","Tân Thạnh","Tân Trụ","Thạnh Hóa","Thủ Thừa","Vĩnh Hưng"}),
                        new City("Nam Định", new string[] {"Giao Thủy","Hải Hậu","Mỹ Lộc","Nam Định","Nam Trực","Nghĩa Hưng","Trực Ninh","Vụ Bản","Xuân Trường","Ý Yên"}),
                        new City("Nghệ An", new string[] {"Anh Sơn","Con Cuông","Cửa Lò","Diễn Châu","Đô Lương","Hoàng Mai","Hưng Nguyên","Kỳ Sơn","Nam Đàn","Nghi Lộc","Nghĩa Đàn","Quế Phong","Quỳ Châu","Quỳ Hợp","Quỳnh Lưu","Tân Kỳ","Thái Hòa","Thanh Chương","Tương Dương","Vinh","Yên Thành"}),
                        new City("Ninh Bình", new string[] {"Gia Viễn","Hoa Lư","Kim Sơn","Nho Quan","Ninh Bình","Tam Điệp","Yên Khánh","Yên Mô"}),
                        new City("Ninh Thuận", new string[] {"Bác Ái","Ninh Hải","Ninh Phước","Ninh Sơn","Phan Rang-Tháp Chàm","Thuận Bắc","Thuận Nam"}),
                        new City("Phú Thọ", new string[] {"Cẩm Khê","Đoan Hùng","Hạ Hòa","Lâm Thao","Phú Thọ","Phù Ninh","Tam Nông","Tân Sơn","Thanh Ba","Thanh Sơn","Thanh Thủy","Việt Trì","Yên Lập"}),
                        new City("Phú Yên", new string[] {"Đông Hòa","Đồng Xuân","Phú Hòa","Sông Cầu","Sông Hinh","Sơn Hòa","Tây Hòa","Tuy An","Tuy Hòa"}),
                        new City("Quảng Bình", new string[] {"Ba Đồn","Bố Trạch","Đồng Hới","Lệ Thủy","Minh Hóa","Quảng Ninh","Quảng Trạch","Tuyên Hóa"}),
                        new City("Quảng Nam", new string[] {"Bắc Trà My","Duy Xuyên","Đại Lộc","Điện Bàn","Đông Giang","Hiệp Đức","Hội An","Nam Giang","Nam Trà My","Nông Sơn","Núi Thành","Phú Ninh","Phước Sơn","Quế Sơn","Tam Kỳ","Tây Giang","Thăng Bình","Tiên Phước"}),
                        new City("Quảng Ngãi", new string[] {"Ba Tơ","Bình Sơn","Đức Phổ","Lý Sơn","Minh Long","Mộ Đức","Nghĩa Hành","Quảng Ngãi","Sơn Hà","Sơn Tây","Sơn Tịnh","Tây Trà","Trà Bồng","Tư Nghĩa"}),
                        new City("Quảng Ninh", new string[] {"Ba Chẽ","Bình Liêu","Cẩm Phả","Cô Tô","Đầm Hà","Đông Triều","Hạ Long","Hải Hà","Hoành Bồ","Móng Cái","Quảng Yên","Tiên Yên","Uông Bí","Vân Đồn"}),
                        new City("Quảng Trị", new string[] {"Cam Lộ","Cồn Cỏ","Đa Krông","Đông Hà","Gio Linh","Hải Lăng","Hướng Hóa","Quảng Trị","Triệu Phong","Vĩnh Linh"}),
                        new City("Sóc Trăng", new string[] {"Châu Thành","Cù Lao Dung","Kế Sách","Long Phú","Mỹ Tú","Mỹ Xuyên","Ngã Năm","Sóc Trăng","Thạnh Trị","Trần Đề","Vĩnh Châu"}),
                        new City("Sơn La", new string[] {"Bắc Yên","Mai Sơn","Mộc Châu","Mường La","Phù Yên","Quỳnh Nhai","Sông Mã","Sốp Cộp","Sơn La","Thuận Châu","Vân Hồ","Yên Châu"}),
                        new City("Tây Ninh", new string[] {"Bến Cầu","Châu Thành","Dương Minh Châu","Gò Dầu","Hòa Thành","Tân Biên","Tân Châu","Tây Ninh","Trảng Bàng"}),
                        new City("Thái Bình", new string[] {"Đông Hưng","Hưng Hà","Kiến Xương","Quỳnh Phụ","Thái Bình","Thái Thụy","Tiền Hải","Vũ Thư"}),
                        new City("Thái Nguyên", new string[] {"Đại Từ","Định Hóa","Đồng Hỷ","Phổ Yên","Phú Bình","Phú Lương","Sông Công","Thái Nguyên","Võ Nhai"}),
                        new City("Thanh Hóa", new string[] {"Bá Thước","Bỉm Sơn","Cẩm Thủy","Đông Sơn","Hà Trung","Hậu Lộc","Hoằng Hóa","Lang Chánh","Mường Lát","Nga Sơn","Ngọc Lặc","Như Thanh","Như Xuân","Nông Cống","Quan Hóa","Quan Sơn","Quảng Xương","Sầm Sơn","Thạch Thành","Thanh Hóa","Thiệu Hóa","Thọ Xuân","Thường Xuân","Tĩnh Gia","Triệu Sơn","Vĩnh Lộc","Yên Định"}),
                        new City("Thừa Thiên - Huế", new string[] {"Huế","Hương Thủy","Hương Trà","Nam Đông","A Lưới","Phong Điền","Phú Lộc","Phú Vang","Quảng Điền"}),
                        new City("Tiền Giang", new string[] {"Cai Lậy","Cai Lậy","Cái Bè","Châu Thành","Chợ Gạo","Gò Công","Gò Công Đông","Gò Công Tây","Mỹ Tho","Tân Phú Đông","Tân Phước"}),
                        new City("TP Hồ Chí Minh", new string[] {"Bình Chánh","Bình Tân","Bình Thạnh","Cần Giờ","Củ Chi","Gò Vấp","Hóc Môn","Nhà Bè","Phú Nhuận","Quận 1","Quận 2","Quận 3","Quận 4","Quận 5","Quận 6","Quận 7","Quận 8","Quận 9","Quận 10","Quận 11","Quận 12","Tân Bình","Tân Phú","Thủ Đức"}),
                        new City("Trà Vinh", new string[] {"Càng Long","Cầu Kè","Cầu Ngang","Châu Thành","Duyên Hải","Duyên Hải","Tiểu Cần","Trà Cú","Trà Vinh"}),
                        new City("Tuyên Quang", new string[] {"Chiêm Hóa","Hàm Yên","Lâm Bình","Na Hang","Sơn Dương","Tuyên Quang","Yên Sơn"}),
                        new City("Vĩnh Long", new string[] {"Bình Minh","Bình Tân","Long Hồ","Mang Thít","Tam Bình","Trà Ôn","Vĩnh Long","Vũng Liêm"}),
                        new City("Vĩnh Phúc", new string[] {"Bình Xuyên","Lập Thạch","Phúc Yên","Sông Lô","Tam Dương","Tam Đảo","Vĩnh Tường","Vĩnh Yên","Yên Lạc"}),
                        new City("Yên Bái", new string[] {"Lục Yên","Mù Căng Chải","Nghĩa Lộ","Trạm Tấu","Trấn Yên","Văn Chấn","Văn Yên","Yên Bái","Yên Bình"})
                    }
                );
            }
        }
    }
}