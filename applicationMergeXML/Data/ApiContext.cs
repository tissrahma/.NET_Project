
using applicationMergeXML.Models;
using Microsoft.EntityFrameworkCore;

namespace applicationMergeXML.Data
   
{
    public class ApiContext : DbContext
    {
        public DbSet<XMLFile> XMLfiles { get; set; }
        public ApiContext(DbContextOptions<ApiContext> options): base(options)
        {

        }
    }
}
