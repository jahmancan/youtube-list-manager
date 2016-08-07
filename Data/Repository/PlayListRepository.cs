using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Data.Repository
{
    public class PlayListRepository : Repository<PlayList>, IPlayListRepository
    {
        public PlayListRepository(YouTubeListManagerContext context) : base(context)
        {
        }

        public void Update(PlayList playList)
        {
            
        }
    }
}
