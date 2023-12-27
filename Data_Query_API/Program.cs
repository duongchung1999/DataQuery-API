using Data_Query_API.DataProcessing.RealTimeDataStatistics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

#region MiniProfiler ���ܷ���
builder.Services.AddMiniProfiler(options =>
options.RouteBasePath = "/profiler"
 );
#endregion

#region Cors��������
builder.Services.AddCors(options =>
{
    options.AddPolicy
        (name: "AllRequests",
            builde =>
            {
                builde.WithOrigins("*", "*", "*")
                .AllowAnyOrigin()  //�����κη���
                .AllowAnyHeader()  // �����κ�����ͷ
                .AllowAnyMethod(); //����Я����֤��Ϣ
            }
        );
});
#endregion

// Add services to the container.
//ScheduledTasks.RealTimeDataReport();// ��ҳ�������ɶ�ʱ��
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Data_Query_API",
        Version = "V1",
        Description = "���ݲ�ѯ API �ĵ�"
    });
    var file = Path.Combine(AppContext.BaseDirectory, "Data_Query_API.xml");  // xml�ĵ�����·��
    var path = Path.Combine(AppContext.BaseDirectory, file); // xml�ĵ�����·��
    c.IncludeXmlComments(path, true); // true : ��ʾ��������ע��
    c.OrderActionsBy(o => o.RelativePath); // ��action�����ƽ�����������ж�����Ϳ��Կ���Ч���ˡ�
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100000000; // ����100M
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

//����UseMiniProfiler
app.UseMiniProfiler();
// ��������
app.UseCors("AllRequests");
app.UseSwagger();
//����SwaggerUI
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Data_Query_API V1");
    c.RoutePrefix = string.Empty;
    c.IndexStream = () => typeof(Program).Assembly.GetManifestResourceStream("Data_Query_API.index.html");
    //������ҳΪSwagger
    c.RoutePrefix = string.Empty;
    //����Ϊ-1 �ɲ���ʾmodels
    c.DefaultModelsExpandDepth(-1);
});

app.UseAuthorization();

app.MapControllers();

app.Run();
