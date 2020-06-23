var utils = {
  parse: function (responseText) {
    try {
      return responseText ? JSON.parse(responseText) : {};
    } catch (e) {
      return {};
    }
  },

  getQueryString: function (name) {
    var result = location.search.match(
      new RegExp('[?&]' + name + '=([^&]+)', 'i')
    );
    if (!result || result.length < 1) {
      return '';
    }
    return decodeURIComponent(result[1]);
  },

  getQueryInt: function(name) {
    var value = utils.getQueryString(name);
    return value ? parseInt(value) : 0;
  },

  getPageUrl: function(fileName) {
    var url = fileName +
      '?pluginId=' + utils.getQueryString('pluginId') +
      '&apiUrl=' + encodeURIComponent(utils.getQueryString('apiUrl'));
    var siteId = utils.getQueryInt('siteId');
    var channelId = utils.getQueryInt('channelId');
    var contentId = utils.getQueryInt('contentId');
    if (siteId > 0) url += '&siteId=' + siteId;
    if (channelId > 0) url += '&channelId=' + channelId;
    if (contentId > 0) url += '&contentId=' + contentId;
    return url;
  },

  getPageAlert: function (error) {
    var message = error.message;
    if (error.response && error.response.data) {
      if (error.response.data.exceptionMessage) {
        message = error.response.data.exceptionMessage;
      } else if (error.response.data.message) {
        message = error.response.data.message;
      }
    }

    return {
      type: "danger",
      html: message
    };
  },

  loading: function (isLoading) {
    if (isLoading) {
      return layer.load(1, {
        shade: [0.2, '#000']
      });
    } else {
      layer.close(layer.index);
    }
  },

  up: function () {
    document.documentElement.scrollTop = document.body.scrollTop = 0;
  },

  closeLayer: function () {
    parent.layer.closeAll();
    return false;
  },

  openLayer: function (config) {
    if (!config || !config.url) return false;

    if (!config.width) {
      config.width = $(window).width() - 50;
    }
    if (!config.height) {
      config.height = $(window).height() - 50;
    }

    if (config.full) {
      config.width = $(window).width() - 50;
      config.height = $(window).height() - 50;
    }

    layer.open({
      type: 2,
      btn: null,
      title: config.title,
      area: [config.width + 'px', config.height + 'px'],
      maxmin: true,
      resize: true,
      shadeClose: true,
      content: config.url
    });

    return false;
  },

  openImagesLayer: function (imageUrls) {
    var data = [];
    for (var i = 0; i < imageUrls.length; i++) {
      var imageUrl = imageUrls[i];
      data.push({
        src: imageUrl, //原图地址
        thumb: imageUrl //缩略图地址
      });
    }
    layer.photos({
      photos: {
        data: data
      },
      anim: 5
    });
  },

  alertDelete: function (config) {
    if (!config) return false;

    swal2({
      title: config.title,
      text: config.text,
      type: 'question',
      confirmButtonText: '确认删除',
      confirmButtonClass: 'btn btn-danger',
      showCancelButton: true,
      cancelButtonText: '取 消'
    }).then(function (result) {
      if (result.value) {
        config.callback();
      }
    });

    return false;
  }
};
