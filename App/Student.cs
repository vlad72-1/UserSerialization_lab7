using System.Runtime.Serialization;

namespace App
{

    [DataContract]
    class Student
    {

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Age { get; set; }

        public string City { get; set; }                             // Переменная "City" передана не будет. //

        public Student (string name, int age, string city)
        {
            Name = name;
            Age = age;
            City = city;
        }
    }
}
