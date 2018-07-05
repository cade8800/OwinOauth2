using Abp.UI;
using Abp.Web.Models;
using Abp.WebApi.Controllers;
using OwnerSayCar.Image;
using OwnerSayCar.Image.Dto;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OwnerSayCar.Web.ApiControllers
{
    public class ImageController : AbpApiController
    {
        private readonly IImageAppService _imageAppService;
        public ImageController(IImageAppService imageAppService)
        {
            _imageAppService = imageAppService;
        }

        /// <summary>
        /// 图片上传 multipart/form-data
        /// </summary>
        /// <returns></returns>
        [WrapResult]
        public async Task<List<UploadImageInput>> ImgFromDataUploadAsync()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new UserFriendlyException("上传格式不是multipart/form-data");

            //创建保存上传文件的物理路径
            var root = System.Web.Hosting.HostingEnvironment.MapPath(GetUploadSaveImgPath());

            //如果路径不存在，创建路径  
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);

            var provider = new MultipartFormDataStreamProvider(root);

            //读取 MIME 多部分消息中的所有正文部分，并生成一组 HttpContent 实例作为结果
            await Request.Content.ReadAsMultipartAsync(provider);

            List<UploadImageInput> uploadFileResultList = new List<UploadImageInput>();

            foreach (var file in provider.FileData)
            {
                //获取上传文件名 这里获取含有双引号'" '
                string fileName = file.Headers.ContentDisposition.FileName.Trim('"');
                //获取上传文件后缀名
                string fileExt = fileName.Substring(fileName.LastIndexOf('.'));

                FileInfo fileInfo = new FileInfo(file.LocalFileName);

                if (fileInfo.Length > 0 && fileInfo.Length <= GetUploadImgMaxByte())
                {
                    if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(GetUploadImgType().Split(','), fileExt.Substring(1).ToLower()) == -1)
                    {
                        fileInfo.Delete();
                        throw new UserFriendlyException("上传的文件格式不支持");
                    }
                    else
                    {
                        UploadImageInput uploadFile = new UploadImageInput();
                        uploadFile.Id = Guid.NewGuid();
                        uploadFile.NAME = fileName;
                        uploadFile.URL = GetUploadSaveImgPath() + uploadFile.Id.ToString() + fileExt;

                        fileInfo.MoveTo(Path.Combine(root, uploadFile.Id.ToString() + fileExt));
                        uploadFileResultList.Add(uploadFile);
                        _imageAppService.UploadImage(uploadFile);
                    }
                }
                else
                {
                    fileInfo.Delete();
                    throw new UserFriendlyException("上传文件的大小不符合");
                }
            }
            return uploadFileResultList;
        }

        private string GetUploadImgType()
        {
            return !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Get("UploadImgType")) ?
                ConfigurationManager.AppSettings.Get("UploadImgType") : "jpg,png,gif";
        }
        private string GetUploadSaveImgPath()
        {
            return !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Get("UploadSaveImgPath")) ?
                ConfigurationManager.AppSettings.Get("UploadSaveImgPath") : "/Resource/Images/";
        }
        private int GetUploadImgMaxByte()
        {
            int.TryParse(ConfigurationManager.AppSettings.Get("UploadImgMaxByte"), out int UploadImgMaxByte);
            return UploadImgMaxByte > 0 ? UploadImgMaxByte : 5242880;
        }
    }
}
