/* eslint-env node */
/* eslint no-console: "off", no-inner-declarations: "off" */
const webpack = require('webpack');
const https = require('https');
const proxy = require('http-proxy-middleware');
const express = require('express');
const webpackDevMiddleware = require('webpack-dev-middleware');
const webpackHotMiddleware = require('webpack-hot-middleware');
const fs = require('fs');
const tls = require('tls');

const config = require('./webpack.config.js')({});

const parentPid = process.argv[2];

function processExists(pid) {
  try {
    // Sending signal 0 - on all platforms - tests whether the process exists. As long as it doesn't
    // throw, that means it does exist.
    process.kill(pid, 0);
    return true;
  }
  catch (ex) {
    // If the reason for the error is that we don't have permission to ask about this process,
    // report that as a separate problem.
    if (ex.code === 'EPERM') {
      throw new Error(
        `Attempted to check whether process ${pid} was running, but got a permissions error.`
      );
    }
    console.log(`\n***** Parent process {pid} not found *****\n`);
    return false;
  }
}

if (parentPid) {
  process.stdout.setDefaultEncoding('latin1');

  setInterval(() => {
    if (!processExists(parentPid)) {
      // Can't log anything at this point, because out stdout was connected to the parent,
      // but the parent is gone.
      process.exit();
    }
  }, 1000);

  // Pressing ctrl+c in the terminal sends a SIGINT to all processes in the foreground process tree.
  // By default, the Node process would then exit before the .NET process, because ASP.NET implements
  // a delayed shutdown to allow ongoing requests to complete.
  //
  // Since this Node script is already set up to shut itself down if it detects the .NET process is
  // terminated, all we have to do is ignore the SIGINT. The Node process will then terminate
  // automatically after the.NET process does.
  process.on('SIGINT', () => {
    console.log('Received SIGINT. Waiting for .NET process to exit...');
  });
}

var app = express();
var compiler = webpack(config);

app.use(webpackHotMiddleware(compiler, {}));

// Rewrite requests for the "home" page to match the path specified in the following middleware.
app.use((req, res, next) => {
  if (req.url === '/circulation') req.url = '/circulation/';
  next();
});

// Now let webpack serve everything it knows about.
app.use(
  webpackDevMiddleware(compiler, {
    contentBase: config.output.path,
    publicPath: config.output.publicPath,

    // Always write html files to disk to allow the backend logic to serve them in support of client-side routing.
    writeToDisk: (filePath) => { return /\.html$/.test(filePath); },

    // replacing the built-in logger with a simplified one that doesn't use any special characters so the output is clean in the VS output window or any console.'
    logger: {
      info: str => console.log(`[WDM] ${str}`),
      error: str => console.error(`[WDM] ${str}`),
      warn: str => console.warn(`[WDM] ${str}`)
    },

    // turning off colored stats to make them readable in the Visual Studio output window.
    //TODO: make this controllable by a parameter so that running this devServer straight from the console still gets the colorful stats
    stats: {colors:false}
  })
);

// If it's within the "/circulation" path, and webpack didn't serve it, then proxy it through to our backend.
app.use(
  '/circulation/',
  proxy({
    target: 'https://localhost:44310/',
    // ignore certificate errors
    secure: false,
    // change the Host header to the target domain name
    changeOrigin: true,
    // include the x-forwarded* headers
    xfwd: true,
    onError: function(err, req) {
      console.log(`proxy error on ${req.method} ${req.path}`);
      console.log(err);
    }
  })
);

// Pass all the remaining requests to the relevant legacy site

// Proxy requests for http(s)://marketplace.*/** to the IFP backend
app.use(
  (req, res, next) => {
    console.log(`\r\nAbout to try the IFP proxy. path = ${req.url}\r\n`);
    next();
  },
  proxy(
    // request context matching filter: https://github.com/chimurai/http-proxy-middleware#context-matching
    (pathname, req) => req.hostname.match('^marketplace'),
    {
      target: 'https://localhost:44307/',
      secure: false,
      changeOrigin: true,
      xfwd: true
    }
  )
);

// Proxy all the rest of the requests to the Medicare backend
app.use(
  (req, res, next) => {
    console.log(`\r\nAbout to proxy to Medicare. path = ${req.url}\r\n`);
    next();
  },
  proxy({
    target: 'https://localhost:44368/',
    secure: false,
    changeOrigin: true,
    xfwd: true
  })
);

// Ass of node v11 or v12, the default minimum is TLSV1.2, but for old IIS Express servers we need to downgrade.
tls.DEFAULT_MIN_VERSION = 'TLSv1';

const httpsOptions = {
  key: fs.readFileSync('devcerts/localhost.key'),
  cert: fs.readFileSync('devcerts/localhost.crt')
};

https.createServer(httpsOptions, app).listen(34310, err => {
  if (err) console.error(err);
  console.log('Webpack Dev Server is running');
});
