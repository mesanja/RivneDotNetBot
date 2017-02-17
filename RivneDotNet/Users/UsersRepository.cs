using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Serialization;

namespace RivneDotNet.Users
{
    public class UsersRepository
    {
        private readonly string fullFilePath;

        public UsersRepository()
        {
            var folder = HostingEnvironment.MapPath("~/App_Data");
            if (folder != null)
            {
                Directory.CreateDirectory(folder);
                fullFilePath = Path.Combine(folder,"users.xml");
            }
        }

        public IQueryable<UserDto> GetAlll()
        {
            List<UserDto> users = null;

            if (File.Exists(fullFilePath))
            {
                users = Load(fullFilePath);
            }
            else
            {
                users = new List<UserDto>();
            }

            return users.AsQueryable();
        }

        public void AddUser(UserForm user)
        {
            List<UserDto> users = null;
            if (File.Exists(fullFilePath))
            {
                users = Load(fullFilePath);
            }
            else
            {
                users = new List<UserDto>();
            }
            users.Add(new UserDto
            {
                Date = DateTime.Now,
                Email = user.Email,
                FullName = user.FullName,
                Gender = user.Gender
            });
            Save(fullFilePath, users);
        }

        public static List<UserDto> Load(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var serializer = new XmlSerializer(typeof(List<UserDto>));
                return serializer.Deserialize(stream) as List<UserDto>;
            }
        }

        public void Save(string fileName, List<UserDto> user)
        {
            //first serialize the object to memory stream,
            //in case of exception, the original file is not corrupted
            using (MemoryStream ms = new MemoryStream())
            {
                var writer = new StreamWriter(ms);
                var serializer = new XmlSerializer(user.GetType());
                serializer.Serialize(writer, user);
                writer.Flush();

                //if the serialization succeed, rewrite the file.
                File.WriteAllBytes(fileName, ms.ToArray());
            }
        }
    }
}