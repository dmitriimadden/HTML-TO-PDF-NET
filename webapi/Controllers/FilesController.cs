using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using PuppeteerSharp;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Web.Http.Cors;

namespace webapi.Controllers;


[ApiController]
[Route("[controller]/[action]")]
[EnableCors(origins: "*", headers: "*", methods: "*")]
public class FilesController : ControllerBase
{
    string pathInput = Path.Combine("./files/", "input");
    string pathOutput = Path.Combine("./files/", "output");

    [HttpPost()]
    public async Task<IActionResult> Upload()
    {
        var files = Request.Form.Files;

        foreach (var file in files)
        {
            if (file.FileName.Contains(".html"))
            {
                var fileName = file.FileName;
                var path = Path.Combine("./files/", "input");
                var pathPath = Path.Combine(path, fileName);
                string errorMsg = string.Empty;

                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var stream = new FileStream(pathPath, FileMode.Create);

                    await file.CopyToAsync(stream);
                    await stream.FlushAsync();
                    stream.Close();

                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }
                finally
                {
                }

            }
            else 
                return StatusCode(400);
        }
        return new JsonResult(files.ToList());

    }
    [HttpPost("{fileName}")]
    public async Task<IActionResult> DeleteFile(string fileName)
    {
        string errorMsg = string.Empty;
        try
        {

            var html = @$"./files/input/{fileName}";
            var pdf = @$"./files/output/{fileName.Replace("html", "pdf")}";

            if (System.IO.File.Exists(html))
            {
                System.IO.File.Delete(html);
            }
            if (System.IO.File.Exists(pdf))
            {
                System.IO.File.Delete(pdf);
            }

        }
        catch (Exception ex)
        {
            errorMsg = ex.Message;
            return StatusCode(400);

        }
        finally
        {
        }

        return StatusCode(200);

    }

    [HttpPost("{fileName}")]
    public async Task<IActionResult> ConvertFile(string fileName)
    {
        var pathPath = @$"./files/input/{fileName}";
        string errorMsg = string.Empty;
        using var browserFetcher = new BrowserFetcher();
        IBrowser browser;
        try
        {
            await browserFetcher.DownloadAsync();
            browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            var html = System.IO.File.ReadAllText(pathPath);

            using (var page = await browser.NewPageAsync())
            {
                await page.SetContentAsync(html);
                var result = await page.GetContentAsync();
                var newPath = @$"./files/output/{fileName.Replace("html", "pdf")}";
                if (!Directory.Exists("./files/output/"))
                {
                    Directory.CreateDirectory("./files/output/");
                }
                await page.PdfAsync(newPath);
            }
             await browser.CloseAsync();
             browser.Dispose(); 
        }
        catch (Exception ex)
        {
            errorMsg = ex.Message;
            return StatusCode(400);

        }
        finally
        {
            browserFetcher.Dispose();
        }

        return StatusCode(200);

    }

    [HttpGet()]
    public async Task<IActionResult> GetInputFiles()
    {
        string[] fileArrayInput =  new string[] { };
        string[] fileArrayOutput = new string[] { };
        if (Directory.Exists(pathInput))
            fileArrayInput = Directory.GetFiles(pathInput, "*.html");
       
        if (Directory.Exists(pathOutput))
            fileArrayOutput = Directory.GetFiles(pathOutput);

        var HotsUrl = Request.Scheme + "://" + Request.Host.Value;

        List<FileModel> fileArrayEx = new List<FileModel>();
        foreach (string file in fileArrayInput)
        {
            var exFile = new FileModel()
            {
                Name = file.Substring(file.IndexOf("\\") + 1),
                Converted = fileArrayOutput.Contains(file.Replace("input", "output").Replace("html", "pdf")),
                Modified =  System.IO.File.GetLastWriteTime(file),
        };
            exFile.Link = exFile.Converted ? (HotsUrl + @$"/files/download/{exFile.Name.Replace("html", "pdf")}") : "don't have link";
            fileArrayEx.Add(exFile);
        }
        fileArrayEx = fileArrayEx.OrderByDescending(x => x.Modified).ToList();

            return new JsonResult(fileArrayEx.ToList());
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> Download(string fileName)
    {
        var pathPath = @$"./files/output/{fileName}";
        if (System.IO.File.Exists(pathPath))
        {

            var file = System.IO.Path.GetFileName(pathPath);
            var content = await System.IO.File.ReadAllBytesAsync(pathPath);
            new FileExtensionContentTypeProvider()
                .TryGetContentType(file, out string contentType);
            return File(content, contentType, file);
        }
        else
            return StatusCode(400);

    }

}