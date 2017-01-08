using Nancy;
using System;
using System.Collections.Generic;
using DbConnection;
using System.Text.RegularExpressions;
using CryptoHelper;


namespace Login
{
   
    public class LoginModule : NancyModule
    {
        public LoginModule()
        {
            Get("/", args =>
            {
                List<string> errors= new List<string>();
                if(Session["errors"]!= null)
                {
                    errors = (List<string>)Session["errors"];
                    ViewBag.errors = true;
                    Session.DeleteAll();
                }
                
                return View["login.sshtml", errors];
            });

            Post("/login", args =>
            {
                string email = (string)Request.Form.login_email;
                string password = (string)Request.Form.login_password;
                string Query = $"SELECT id, password FROM users WHERE email= '{email}'";
                List<Dictionary<string, object>> result = DbConnector.ExecuteQuery(Query);
                if(result.Count<1)
                {
                    List<string> errors = new List<string>();
                    errors.Add("The username or password does not match one on file");
                    Session["errors"] = errors;
                    return Response.AsRedirect("/");
                }
                string encryptedString = (string)result[0]["password"];
                if(Crypto.VerifyHashedPassword(encryptedString, password))
                {
                    Session["id"] = (int)result[0]["id"];
                    ViewBag.id = (int)Session["id"];
                    return View["success"];
                }
                else{
                    List<string> errors = new List<string>();
                    errors.Add("The username or password does not match one on file");
                    Session["errors"] = errors;
                    return Response.AsRedirect("/");

                }
                
            });

            Post("/register", args =>
            {
                 List<string> errors = new List<string>();
                 if(((string)Request.Form.first_name).Length< 2)
                 {
                     errors.Add("First Name not long enough");
                 }
                  if(((string)Request.Form.last_name).Length< 2)
                 {
                     errors.Add("Last Name not long enough");
                 }
                if(((string)Request.Form.password).Length< 8)
                 {
                     errors.Add("Password not long enough");
                 }
                if(((string)Request.Form.confirm_password)!=((string)Request.Form.password))
                 {
                     errors.Add("Passwords do not match!");
                 }
                if(Regex.IsMatch(((string)Request.Form.first_name), @"^[\p{L}]+$")==false && ((string)Request.Form.first_name).Length>0)
                 {
                     errors.Add("Name Cannot contain numbers!");
                 }
                if(Regex.IsMatch(((string)Request.Form.last_name), @"^[\p{L}]+$")==false && ((string)Request.Form.last_name).Length>0)
                 {
                     errors.Add("Name Cannot contain numbers!");
                 }
                if(Regex.IsMatch(((string)Request.Form.email), @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                        RegexOptions.IgnoreCase)==false)
                 {
                     errors.Add("Enter a valid email address!");
                 }
                if(((string)Request.Form.email).Length< 1)
                 {
                     errors.Add("Email is required.");
                 }
                 if(errors.Count>0)
                 {
                     Session["errors"] = errors;
                    return Response.AsRedirect("/");
                 }
                 else{
                     string EncryptedPassword = Crypto.HashPassword(((string)Request.Form.confirm_password));
                     string first_name = ((string)Request.Form.first_name);
                     string last_name = ((string)Request.Form.last_name);
                     string email = ((string)Request.Form.email);
                     string Query = $"INSERT INTO users (first_name, last_name, email, password, created_at, updated_at) VALUES ('{first_name}', '{last_name}', '{email}', '{EncryptedPassword}', NOW(), NOW())";
                     DbConnector.ExecuteQuery(Query);
                     string getId = $"SELECT id FROM users WHERE email= '{email}'";
                     List<Dictionary<string, object>> result = DbConnector.ExecuteQuery(getId);
                     Session["id"] = (int)result[0]["id"];
                     ViewBag.id = (int)Session["id"];
                     return View["success"];
                 }
            });
        }
    }
}

