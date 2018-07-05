using Abp.UI;
using Abp.Web.Models;
using Abp.WebApi.Controllers;
using OwnerSayCar.Video;
using OwnerSayCar.Video.Dto;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OwnerSayCar.Web.ApiControllers
{
    public class VideoController : AbpApiController
    {
        private readonly IVideoAppService _videoAppService;
        public VideoController(IVideoAppService videoAppService)
        {
            _videoAppService = videoAppService;
        }

        /// <summary>
        /// 图片上传 multipart/form-data
        /// </summary>
        /// <returns></returns>
        [WrapResult]
        public async Task<List<UploadVideoInput>> VideoFromDataUploadAsync()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new UserFriendlyException("上传格式不是multipart/form-data");

            //创建保存上传文件的物理路径
            var root = System.Web.Hosting.HostingEnvironment.MapPath(GetUploadSaveVideoPath());

            //如果路径不存在，创建路径  
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);

            var provider = new MultipartFormDataStreamProvider(root);

            //读取 MIME 多部分消息中的所有正文部分，并生成一组 HttpContent 实例作为结果
            await Request.Content.ReadAsMultipartAsync(provider);

            List<UploadVideoInput> uploadFileResultList = new List<UploadVideoInput>();

            foreach (var file in provider.FileData)
            {
                //获取上传文件名 这里获取含有双引号'" '
                string fileName = file.Headers.ContentDisposition.FileName.Trim('"');
                //获取上传文件后缀名
                string fileExt = fileName.Substring(fileName.LastIndexOf('.'));

                FileInfo fileInfo = new FileInfo(file.LocalFileName);

                if (fileInfo.Length > 0 && fileInfo.Length <= GetUploadVideoMaxByte())
                {
                    if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(GetUploadVideoType().Split(','), fileExt.Substring(1).ToLower()) == -1)
                    {
                        fileInfo.Delete();
                        throw new UserFriendlyException("上传的文件格式不支持");
                    }
                    else
                    {
                        UploadVideoInput uploadFile = new UploadVideoInput();
                        uploadFile.Id = Guid.NewGuid();
                        uploadFile.NAME = fileName;
                        uploadFile.URL = GetUploadSaveVideoPath() + uploadFile.Id.ToString() + fileExt;

                        fileInfo.MoveTo(Path.Combine(root, uploadFile.Id.ToString() + fileExt));
                        uploadFileResultList.Add(uploadFile);
                        _videoAppService.UploadVideo(uploadFile);
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

        private string GetUploadVideoType()
        {
            return !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Get("UploadVideoType")) ?
                ConfigurationManager.AppSettings.Get("UploadVideoType") : "mp4,";
        }
        private string GetUploadSaveVideoPath()
        {
            return !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Get("UploadSaveVideoPath")) ?
                ConfigurationManager.AppSettings.Get("UploadSaveVideoPath") : "/Resource/Videos/";
        }
        private int GetUploadVideoMaxByte()
        {
            int.TryParse(ConfigurationManager.AppSettings.Get("UploadVideoMaxByte"), out int UploadVideoMaxByte);
            return UploadVideoMaxByte > 0 ? UploadVideoMaxByte : 5242880;
        }
    }
}
