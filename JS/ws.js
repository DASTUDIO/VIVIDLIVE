const dgram = require('dgram');
const ws = require('ws');

var u = dgram.createSocket('udp4');
const w = new ws.WebSocketServer({ port: 8087 });
w.on('connection', function connection(ws) {
  ws.on('message', function message(data) {
    u.send(data, 8089, '127.0.0.1', function (err, bytes) {
      if (err) throw err;
    });
  });
});
