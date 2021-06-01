using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AuthPracticeLibrary
{
    public class PersonModel 
    { 
    
        [BsonId] // mongo _id
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        /// <summary>
        /// The iterations, salt, and resulting hash must be stored here separated by a period in the form "iterations.salt.passwordHash"
        /// For example: "10000.Dvsge98yPQCkqSyxgkKZZA==.DMxNfQFQxDgwX3tSIgNijOr7+uFkZqVlmdZJHBMejdM="
        /// </summary>
        public string PasswordHash { get; set; }
        public string PersonRole { get; set; }
        [BsonElement("dob")]
        public DateTime DateOfBirth { get; set; }
    }
}
