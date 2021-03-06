﻿using FileUploadFrameworkAPI.Models;
using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Configuration;
using DataAccessLayer;

namespace FileUploadFrameworkAPI.Helper
{
    public class UploadFileHelper
    {
        public static FileStatusModel ToLocalFolderHelper(HttpPostedFileBase fileBase)
        {
            var sizeConversion = (fileBase.ContentLength / 1024f) / 1024f;
            string directoryPath = string.Empty;
            string sourcePath = string.Empty;

            FileStatusModel fsm = new FileStatusModel()
            {
                FileName = fileBase.FileName,
                FileSize = sizeConversion + " Mb",
            };

            try
            {
                var uploadFolderPath = WebConfigurationManager.AppSettings["UploadFolderPath"].ToString();

                string fileExtension = Path.GetExtension(fileBase.FileName).ToLower().Trim();

                if (!string.IsNullOrEmpty(uploadFolderPath) && !string.IsNullOrEmpty(fileExtension))
                {
                    directoryPath = Path.Combine(uploadFolderPath, DateTime.Now.ToString(WebConfigurationManager.AppSettings["DateFormat"].ToString(), CultureInfo.InvariantCulture));

                    if (fileBase.ContentLength < 50000000)
                    {
                        if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".pdf" || fileExtension == ".xlsx")
                        {
                            sourcePath = directoryPath + "\\" + fileBase.FileName;

                            if (sourcePath.Length < 260)
                            {
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                fileBase.SaveAs(sourcePath);

                                fsm.SourcePath = sourcePath;
                                fsm.StatusMessage = "Uploaded Successfully!";
                                fsm.IsSuccessfull = true;
                            }
                            else
                            {
                                fsm.StatusMessage = "The specified file path is too long. The fully qualified file path must be less than 260 characters.";
                                fsm.IsSuccessfull = false;
                            }
                        }
                        else
                        {
                            fsm.StatusMessage = "Only Image(.jpg/.jpeg), PDF and Excel files are allowed.";
                            fsm.IsSuccessfull = false;
                        }
                    }
                    else
                    {
                        fsm.StatusMessage = "Only 50Mb or lesser files are allowed.";
                        fsm.IsSuccessfull = false;
                    }
                }
                else
                {
                    fsm.StatusMessage = "Invalid File.";
                    fsm.IsSuccessfull = false;
                }
            }
            catch (Exception ex)
            {
                fsm.StatusMessage = ex.Message;
                fsm.IsSuccessfull = false;
            }

            return fsm;
        }

        public static FileStatusModel ToDatabaseHelper(HttpPostedFileBase fileBase)
        {
            var sizeConversion = (fileBase.ContentLength / 1024f) / 1024f;

            FileStatusModel fsm = new FileStatusModel()
            {
                FileName = fileBase.FileName,
                FileSize = sizeConversion + " Mb",
            };

            try
            {
                string fileExtension = Path.GetExtension(fileBase.FileName).ToLower().Trim();

                if (!string.IsNullOrEmpty(fileExtension))
                {
                    if (fileBase.ContentLength < 2000000)
                    {
                        if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".pdf" || fileExtension == ".xlsx")
                        {
                            byte[] fileBinary;

                            using (MemoryStream ms = new MemoryStream())
                            {
                                fileBase.InputStream.CopyTo(ms);
                                fileBinary = ms.ToArray();
                            }

                            DataAccessLayer.File file = new DataAccessLayer.File()
                            {
                                Id = Guid.NewGuid(),
                                FileName = fileBase.FileName,
                                InputStream = fileBinary,
                                ContentType = fileBase.ContentType,
                                ContentLength = fileBase.ContentLength,
                                UploadedDate = DateTime.UtcNow
                            };

                            FilesUploadEntities fue = new FilesUploadEntities();
                            fue.Files.Add(file);
                            int Id = fue.SaveChanges();

                            fsm.Id = file.Id;
                            fsm.SourcePath = "Download";
                            fsm.StatusMessage = "Uploaded Successfully!";
                            fsm.IsSuccessfull = true;
                        }
                        else
                        {
                            fsm.StatusMessage = "Only Image(.jpg/.jpeg), PDF and Excel files are allowed.";
                            fsm.IsSuccessfull = false;
                        }
                    }
                    else
                    {
                        fsm.StatusMessage = "Only 50Mb or lesser files are allowed.";
                        fsm.IsSuccessfull = false;
                    }
                }
                else
                {
                    fsm.StatusMessage = "Invalid File.";
                    fsm.IsSuccessfull = false;
                }
            }
            catch (Exception ex)
            {
                fsm.StatusMessage = ex.Message;
                fsm.IsSuccessfull = false;
            }

            return fsm;
        }
    }
}