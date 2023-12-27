using Data_Query_API.DataProcessing.RealTimeDataStatistics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

#region MiniProfiler 性能分析
builder.Services.AddMiniProfiler(options =>
options.RouteBasePath = "/profiler"
 );
#endregion

#region Cors跨域请求
builder.Services.AddCors(options =>
{
    options.AddPolicy
        (name: "AllRequests",
            builde =>
            {
                builde.WithOrigins("*", "*", "*")
                .AllowAnyOrigin()  //允许任何方法
                .AllowAnyHeader()  // 允许任何请求头
                .AllowAnyMethod(); //允许携带认证信息
            }
        );
});
#endregion

// Add services to the container.
//ScheduledTasks.RealTimeDataReport();// 首页数据生成定时器
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Data_Query_API",
        Version = "V1",
        Description = "数据查询 API 文档"
    });
    var file = Path.Combine(AppContext.BaseDirectory, "Data_Query_API.xml");  // xml文档绝对路径
    var path = Path.Combine(AppContext.BaseDirectory, file); // xml文档绝对路径
    c.IncludeXmlComments(path, true); // true : 显示控制器层注释
    c.OrderActionsBy(o => o.RelativePath); // 对action的名称进行排序，如果有多个，就可以看见效果了。
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100000000; // 限制100M
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

//激活UseMiniProfiler
app.UseMiniProfiler();
// 启动跨域
app.UseCors("AllRequests");
app.UseSwagger();
//配置SwaggerUI
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Data_Query_API V1");
    c.RoutePrefix = string.Empty;
    c.IndexStream = () => typeof(Program).Assembly.GetManifestResourceStream("Data_Query_API.index.html");
    //设置首页为Swagger
    c.RoutePrefix = string.Empty;
    //设置为-1 可不显示models
    c.DefaultModelsExpandDepth(-1);
});

app.UseAuthorization();

app.MapControllers();

app.Run();
