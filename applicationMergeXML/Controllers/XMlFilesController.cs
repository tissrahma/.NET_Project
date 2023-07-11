using Microsoft.AspNetCore.Mvc;
using applicationMergeXML.Data;
using applicationMergeXML.Models;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Xml.Linq;

namespace applicationMergeXML.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class XMlFilesController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<XMlFilesController> _logger;

        public XMlFilesController(ApiContext context, IConfiguration configuration, ILogger<XMlFilesController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;

        }

        // POST: api/XMlFiles
        [HttpPost]

        public IActionResult Add(XMLFile xMLFile)
        {
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string sql = "INSERT INTO XMLFiles (FileName, FileContent) VALUES (@FileName, @FileContent)";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@FileName", xMLFile.fileid);
                    command.Parameters.AddWithValue("@FileContent", xMLFile.name);
                    command.ExecuteNonQuery();
                }
            }

            return Ok(xMLFile);
        }


        // GET: api/XMlFiles
        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _context.XMLfiles.ToList();

            return Ok(result);
        }

        // GET: api/XMlFiles/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = _context.XMLfiles.Find(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // DELETE: api/XMlFiles/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var result = _context.XMLfiles.Find(id);

            if (result == null)
                return NotFound();

            _context.XMLfiles.Remove(result);
            _context.SaveChanges();

            return NoContent();
        }
/////////////////////////
        [HttpPost("merge")]
        public async Task<IActionResult> Merge(List<IFormFile> files)

        {
            if (files != null && files.Count == 2)
            {
                IFormFile file1 = files[0];
                IFormFile file2 = files[1];

                if (file1.Length > 0 && file2.Length > 0)
                {
                    // Merge the XML files
                    XDocument mergedXml = MergeXmlFiles(file1, file2);

                    // Save the merged XML file to the specified directory
                    string savePath = "C:\\Users\\HP\\Desktop\\here\\";
                    string mergedFileName = "merged.xml";
                    string filePath = Path.Combine(savePath, mergedFileName);
                    mergedXml.Save(filePath);

                    string connectionString = _configuration.GetConnectionString("MySqlConnection");
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "INSERT INTO Files (FileName, FilePath) VALUES (@FileName, @FilePath)";
                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@FileName", mergedFileName);
                            command.Parameters.AddWithValue("@FilePath", filePath);
                            command.ExecuteNonQuery();
                        }
                    }

                    return Ok("Files uploaded and merged successfully!");
                }

                return BadRequest("One or both files are empty.");
            }

            return BadRequest("Please provide exactly two files.");
        }

        private XDocument MergeXmlFiles(IFormFile file1, IFormFile file2)
        {
            XDocument xml1 = XDocument.Load(file1.OpenReadStream());
            XDocument xml2 = XDocument.Load(file2.OpenReadStream());

            // Merge the XML files by deleting duplicate lines and preserving the original order
            XDocument mergedXml = new XDocument(xml1);

            foreach (XElement element in xml2.Root.Elements())
            {
                if (!mergedXml.Descendants(element.Name).Any(x => XNode.DeepEquals(x, element)))
                {
                    XElement existingElement = mergedXml.Descendants(element.Name).FirstOrDefault();
                    if (existingElement != null)
                    {
                        existingElement.AddAfterSelf(element);
                    }
                    else
                    {
                        mergedXml.Root.Add(element);
                    }
                }
            }

            return mergedXml;
        }




        /*  public async Task<IActionResult> Upload(IFormFile file)
          {
              if (file != null && file.Length > 0)
              {
                  using (var stream = new MemoryStream())
                  {
                      await file.CopyToAsync(stream);
                      byte[] fileBytes = stream.ToArray();

                      string connectionString = _configuration.GetConnectionString("MySqlConnection");
                      using (MySqlConnection connection = new MySqlConnection(connectionString))
                      {
                          connection.Open();
                          string sql = "INSERT INTO Files (FileName, FileContent) VALUES (@FileName, @FileContent)";
                          using (MySqlCommand command = new MySqlCommand(sql, connection))
                          {
                              command.Parameters.AddWithValue("@FileName", file.FileName);
                              command.Parameters.AddWithValue("@FileContent", fileBytes);
                              command.ExecuteNonQuery();
                          }
                      }
                  }

                  return Ok("File uploaded successfully!");
              }

              return BadRequest("No file or empty file provided.");
          }
          */
    }
}


