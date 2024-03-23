- Khi lấy về nhớ mở package manager console và paste:
- Scaffold-DbContext "Server=YourDBName;Database=DLCT;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -force
thay tên Server bằng tên Server DB của mình sau đó enter để upate lại DB
- Sau đó lên Github https://github.com/Hiephuynh2410/DoAn mở Model vào Cart.cs copy đoạn


        [NotMapped]

        public double? TotalAmount

        {

                get    
                { 
                   if (Quantity.HasValue && Product != null)
                
                {
                    return Quantity.Value * Product.Price;
                }
            
                return null;}
        
                private set { }
          }
        
        
            public void UpdateTotalAmount()
        
            {
                if (Quantity.HasValue && Product != null)
                
                {
                    TotalAmount = Quantity.Value * Product.Price;
                }
                else
                {
                    TotalAmount = null;
                }
            }
 
- Ok chạy DoAn
