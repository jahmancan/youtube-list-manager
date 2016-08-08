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

        public void InsertUpdate(PlayList playList)
        {
            PlayList foundPlayList = FindBy(p => p.Hash == playList.Hash).FirstOrDefault();
            if (foundPlayList == null)
            {
                foundPlayList = Create();
                Insert(foundPlayList);
            }
            
            foundPlayList.Hash = playList.Hash;
            foundPlayList.Title = playList.Title;
            foundPlayList.PrivacyStatus = playList.PrivacyStatus;
            foundPlayList.UserId = playList.UserId;
        }
    }
}
