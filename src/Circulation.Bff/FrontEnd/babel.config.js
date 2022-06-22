const formatMessageConfig = {
  extractFormatMessage: false,
  transformFormatMessage: false
};

const wtwConfig = {
  ...formatMessageConfig,
  env: {
    useBuiltIns: 'usage'
  }
};

module.exports = {
  presets: [['wtw-im', wtwConfig]],
  plugins: []
};
