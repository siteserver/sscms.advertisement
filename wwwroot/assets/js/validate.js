Vue.use(VeeValidate);
VeeValidate.Validator.localize('zh_CN');
VeeValidate.Validator.localize({
  zh_CN: {
    messages: {
      required: function (name) {
        return name + '不能为空';
      }
    }
  }
});
VeeValidate.Validator.extend('mobile', {
  getMessage: function () {
    return ' 请输入正确的手机号码';
  },
  validate: function (value, args) {
    return (
      value.length == 11 &&
      /^((13|14|15|16|17|18|19)[0-9]{1}\d{8})$/.test(value)
    );
  }
});
