using System.Reflection;
using CorrugatedIron;
using CorrugatedIron.Models;
using Tweets.Attributes;
using Tweets.ModelBuilding;
using Tweets.Models;

namespace Tweets.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string bucketName;
        private readonly IRiakClient riakClient;
        private readonly IMapper<User, UserDocument> userDocumentMapper;
        private readonly IMapper<UserDocument, User> userMapper;

        public UserRepository(IRiakClient riakClient, IMapper<User, UserDocument> userDocumentMapper, IMapper<UserDocument, User> userMapper)
        {
            this.riakClient = riakClient;
            this.userDocumentMapper = userDocumentMapper;
            this.userMapper = userMapper;
            bucketName = typeof (UserDocument).GetCustomAttribute<BucketNameAttribute>().BucketName;
        }
        public void Save(User user)
        {
            var userDocument = userDocumentMapper.Map(user);
            var riakObject = new RiakObject(bucketName, userDocument.Id, userDocument);
            riakClient.Put(riakObject);
        }

        public User Get(string userName)
        {
            var riakResult = riakClient.Get(bucketName, userName);
            if (!riakResult.IsSuccess)
                return null;
            var userDocument = riakResult.Value.GetObject<UserDocument>();
            return userMapper.Map(userDocument);
        }
    }
}