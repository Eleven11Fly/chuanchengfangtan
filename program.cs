var builder = WebApplication.CreateBuilder(args);

// 注册 MVC 和 API 控制器
builder.Services.AddControllersWithViews();

// 注册 CardService 为单例（内存存储，进程内共享）
builder.Services.AddSingleton<CCBInteractiveApp.Services.CardService>();

var app = builder.Build();

// 启用静态文件（wwwroot 目录）
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// 映射 API 控制器
app.MapControllers();

// 默认路由（非必需，但保留）
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
