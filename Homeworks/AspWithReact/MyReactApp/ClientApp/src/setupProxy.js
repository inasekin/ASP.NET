const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function(app) {
  app.use(
      '/weatherforecast',
      createProxyMiddleware({
        target: 'https://localhost:7095',
        changeOrigin: true,
        secure: false,
        onError: (err, req, res) => {
          console.error(`Proxy error: ${err.message}`);
          res.status(500).send('Proxy error');
        },
      })
  );
};