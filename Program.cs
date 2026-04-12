using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minecraft.Data;  // Nhớ kiểm tra namespace này đúng với project của bạn
using Minecraft.Models; // Nhớ kiểm tra namespace này đúng với project của bạn
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var dbPath = Path.Combine(env.ContentRootPath, "wwwroot", "data", "MinecraftDB.db");
// ====================================================
// 1. CẤU HÌNH SERVICES (DATABASE, AUTH, SWAGGER...)
// ====================================================

// A. Kết nối Database SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// B. Cấu hình Controller & JSON (Xử lý lỗi vòng lặp dữ liệu)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Dòng này giúp tránh lỗi "Cycle detected" khi trả về JSON có quan hệ bảng
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// C. Cấu hình JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,   // Tắt check Issuer để dễ test
        ValidateAudience = true, // Tắt check Audience để dễ test
        ValidateLifetime = true,  // Kiểm tra token hết hạn chưa
        ValidateIssuerSigningKey = true, // Bắt buộc kiểm tra chữ ký

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // QUAN TRỌNG: Key này phải giống hệt trong appsettings.json và dài >= 32 ký tự
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    // Sự kiện Debug (In lỗi ra màn hình đen Console để dễ sửa)
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine(">>>>> LỖI TOKEN: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine(">>>>> TOKEN HỢP LỆ! User đã đăng nhập.");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            // Console.WriteLine(">>>>> Token nhận được: " + context.Token);
            return Task.CompletedTask;
        }
    };
});

// D. Cấu hình Swagger (Chế độ HTTP Bearer - Không cần gõ tay chữ Bearer)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServerMinecraft", Version = "v1" });

    // Định nghĩa bảo mật kiểu HTTP (Token)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập Token vào ô bên dưới (Chỉ cần Paste token, KHÔNG cần gõ chữ Bearer)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, // Dùng Http thay vì ApiKey để chuẩn hơn
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// ====================================================
// 2. CẤU HÌNH MIDDLEWARE PIPELINE (THỨ TỰ RẤT QUAN TRỌNG)
// ====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Có thể mở lại nếu chạy HTTPS
app.UseStaticFiles();

app.UseRouting(); // 1. Định tuyến

// --- BẮT BUỘC PHẢI CÓ 2 DÒNG NÀY THEO ĐÚNG THỨ TỰ ---
app.UseAuthentication(); // 2. Kiểm tra danh tính (Decode Token)
app.UseAuthorization();  // 3. Kiểm tra quyền hạn (Cho phép vào hay không)
// -----------------------------------------------------

// Cấu hình hiển thị Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServerMinecraft v1"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Tự động cập nhật Database khi chạy (Migration)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
     db.Database.Migrate(); // Mở dòng này nếu muốn tự động update DB
}

app.Run();