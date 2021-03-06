using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DBRepository
{
    public class BlogRepository : BaseRepository, IBlogRepository
    {
        public BlogRepository(string connectionString, IRepositoryContextFactory contextFactory)
            : base(connectionString, contextFactory)
        {
        }

        public async Task<Page<Post>> GetPosts(int index, int pageSize, string tag)
        {
            var result = new Page<Post>() {CurrentPage = index, PageSize = pageSize};

            using (var context = ContextFactory.CreateDbContext(ConnectionString))
            {
                var query = context.Posts.AsQueryable();
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    query = query.Where(p => p.Tags.Any(t => t.TagName == tag));

                    result.TatalPages = await query.CountAsync();

                    query.Include(p => p.Tags)
                        .Include(p => p.Comments)
                        .OrderByDescending(p => p.CreateDate)
                        .Skip(index * pageSize)
                        .Take(pageSize);

                    result.Records = await query.ToListAsync();
                }
            }

            return result;
        }
    }
}