using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using Tweets.ModelBuilding;
using Tweets.Models;

namespace Tweets.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly string connectionString;
        private readonly AttributeMappingSource mappingSource;
        private readonly IMapper<Message, MessageDocument> messageDocumentMapper;
        private readonly DataContext dataContext;

        public MessageRepository(IMapper<Message, MessageDocument> messageDocumentMapper)
        {
            this.messageDocumentMapper = messageDocumentMapper;
            mappingSource = new AttributeMappingSource();
            connectionString = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
            dataContext = new DataContext(connectionString, mappingSource);
        }

        public void Save(Message message)
        {
            var messageDocument = messageDocumentMapper.Map(message);
            dataContext.GetTable<MessageDocument>().InsertOnSubmit(messageDocument);
            dataContext.SubmitChanges();
        }

        public void Like(Guid messageId, User user)
        {
            var likeDocument = new LikeDocument {MessageId = messageId, UserName = user.Name, CreateDate = DateTime.UtcNow};
            dataContext.GetTable<LikeDocument>().InsertOnSubmit(likeDocument);
            dataContext.SubmitChanges();
        }

        public void Dislike(Guid messageId, User user)
        {
            var table = dataContext.GetTable<LikeDocument>();
            table.DeleteAllOnSubmit(table.Where(d => d.MessageId == messageId && d.UserName == user.Name));
            dataContext.SubmitChanges();
        }

        public IEnumerable<Message> GetPopularMessages()
        {
            var messages = dataContext.GetTable<MessageDocument>();
            var likes = dataContext.GetTable<LikeDocument>();

            return messages
                .GroupJoin(likes,
                    m => m.Id,
                    l => l.MessageId,
                    (m, ll) => new Message
                    {
                        Id = m.Id,
                        CreateDate = m.CreateDate,
                        Text = m.Text,
                        User = new User {Name = m.UserName}, // ?
                        Likes = ll.Count()
                    })
                .OrderByDescending(m => m.Likes)
                .Take(10)
                .ToArray();
        }

        public IEnumerable<UserMessage> GetMessages(User user)
        {
            var messages = dataContext.GetTable<MessageDocument>();
            var likes = dataContext.GetTable<LikeDocument>();

            return messages
                .Where(m => m.UserName == user.Name)
                .GroupJoin(likes,
                    m => m.Id,
                    l => l.MessageId,
                    (m, ll) => new UserMessage
                    {
                        Id = m.Id,
                        CreateDate = m.CreateDate,
                        Text = m.Text,
                        User = user,
                        Liked = ll.Any(l => l.UserName == user.Name),
                        Likes = ll.Count()
                    })
                 .OrderByDescending(um => um.CreateDate)
                 .ToArray();
        }
    }
}