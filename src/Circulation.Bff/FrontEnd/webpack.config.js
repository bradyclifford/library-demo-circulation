/* eslint-env node */

const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CleanWebpackPlugin = require('clean-webpack-plugin');
const webpack = require('webpack');

module.exports = (env, argv) => {
  var mode = 'none'; // Local development

  if (argv) {
    mode = argv.mode;
  }

  const outputDir = path.join(__dirname, '..', 'wwwroot');

  const standardplugins = [
      new CleanWebpackPlugin([outputDir], {
        // Need to specify a higher-level project root folder so that
        // this plugin will actually affect the sibling directory.
        root: path.join(__dirname, '..')
      }),
      new HtmlWebpackPlugin({
        template: './App/index.html'
      }),
      new webpack.IgnorePlugin(/^\.\/locale$/, /moment$/)
  ];

  const modeplugins = mode !== 'production' ? [new webpack.HotModuleReplacementPlugin()] : [];

  const entry = ['./App/index.js'];

  const modeentry = mode !== 'production' ? ['webpack-hot-middleware/client'] : [];

  return {
    mode,
    entry: entry.concat(modeentry),
    output: {
      path: outputDir,
      filename: mode !== 'production' ? '[name].[Hash].js' : '[name].[chunkHash].js',
      publicPath: '/circulation/'
    },
    devtool: mode !== 'production' ? 'eval-source-map' : 'none',
    module: {
      rules: [
        {
          test: /\.jsx?$/,
          exclude: /node_modules/,
          use: [
            {
              loader: 'babel-loader',
              options: {
                configFile: './babel.config.js'
              }
            },
            'eslint-loader'
          ]
        },
        {
          test: /\.css$/,
          use: ['style-loader', 'css-loader']
        },
        {
          test: /\.(gif|png|jpe?g|svg)$/i,
          use: [
            {
              loader: 'file-loader'
            },
            {
              loader: 'image-webpack-loader',
              options: {
                disable: true
              }
            }
          ]
        }
      ]
    },
    devServer: {
      watchContentBase: true,
      // Always write html files to disk to allow the backend logic to serve them in support of client-side routing.
      writeToDisk: (filePath) => { return /\.html$/.test(filePath); }
    },
    plugins: standardplugins.concat(modeplugins)
  };
};
