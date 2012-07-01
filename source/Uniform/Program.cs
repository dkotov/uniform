﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using MongoDB.Bson;
using Uniform.Common.Dispatching;
using Uniform.Events;
using Uniform.Storage;
using Uniform.Storage.InMemory;
using Uniform.Storage.Mongodb;

namespace Uniform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var repo = new MongoRepository("mongodb://localhost:27017/local");
            var events = new List<Object>
            {
                new UserCreated("user/1", "Tom", "It's me"),
                new UserCreated("user/2", "John", "Hello!"),
                new QuestionCreated("question/1", "user/1", "Who are you?"),
                new QuestionCreated("question/2", "user/2", "And you?"),
                new UserNameChanged("user/2", "Super John"),
                new QuestionCreated("question/3", "user/2", "How are you?"),
                new QuestionUpdated("question/3", "user/2", "Updated question. How are you?"),
                new CommentAdded("comment/1", "user/1", "question/3", "My first comment!")
            };

            var instance = new MongodbDatabase("mongodb://localhost:27017/local");
            var container = new UnityContainer();
            container.RegisterInstance(repo);
            container.RegisterInstance<IDatabase>(instance);

            var dispatcher = Dispatcher.Create(builder => builder
                .SetServiceLocator(new UnityServiceLocator(container))
                .AddHandlers(typeof(UserCreated).Assembly)
            );

            foreach (var evnt in events)
            {
                dispatcher.Dispatch(evnt);
            }

        }
    }
}