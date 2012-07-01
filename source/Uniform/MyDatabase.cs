﻿using System;
using Uniform.Documents;
using Uniform.Storage;

namespace Uniform
{
    public class MyDatabase
    {
        private readonly IDatabase _database;

        public MyDatabase(IDatabase database)
        {
            _database = database;
        }

        public ICollection<UserDocument> Users
        {
            get { return _database.GetCollection<UserDocument>("Users"); }
        }        
        
        public ICollection<QuestionDocument> Questions
        {
            get { return _database.GetCollection<QuestionDocument>("Questions"); }
        }

        public ICollection<CommentDocument> Comments
        {
            get { return _database.GetCollection<CommentDocument>("Comments"); }
        }


    }
}