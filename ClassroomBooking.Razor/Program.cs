using ClassroomBooking.Repository;
using ClassroomBooking.Repository.UnitOfWork;
using ClassroomBooking.Service.Hubs;
using ClassroomBooking.Service.Interfaces;
using ClassroomBooking.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Data Protection để chia sẻ cookie giữa các ứng dụng
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\SharedKeys"))
    .SetApplicationName("ClassroomBookingApp");

builder.Services.AddDbContext<ClassroomBookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClassroomBookingDB")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ICampusService, CampusService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

builder.Services.AddSignalR();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = "ClassroomBookingAuthCookie";

        // Để Lax hoặc Strict
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Không bắt buộc Secure
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });



builder.Services.AddControllersWithViews(); // Dành cho MVC
builder.Services.AddRazorPages();           // Dành cho Razor Pages

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHub<BookingHub>("/bookingHub");

app.Run();
