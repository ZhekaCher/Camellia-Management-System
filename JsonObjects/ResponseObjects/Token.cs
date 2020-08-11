﻿using System;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CommentTypo

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:37:58
    /// <summary>
    /// Token response while using sign
    /// </summary>
    public class Token : IDisposable
    {
        public string xml { get; set; }
        public long timestamp { get; set; }
        public void Dispose()
        {
            xml = null;
        }
    }
}