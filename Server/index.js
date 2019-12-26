const express = require('express');
const https = require('https');
const app = express();
const fs = require('fs');
const uuid_v4 = require('uuid/v4');

const opts = {
	key: fs.readFileSync('./ssl/privatekey.pem'),
	cert:fs.readFileSync('./ssl/certificate.pem')
}

app.use(express.json());

var debugFilter = new Map();
debugFilter.set('login', true);
debugFilter.set('getData', false);
debugFilter.set('addStack', true);
debugFilter.set('getStack', false);
debugFilter.set('sendData', false);
debugFilter.set('login', false);
debugFilter.set('AddUser', true);

var user = [];
var user_data = {};
var user_stack = new Map();


app.get('/login', function (req, res) {
	res.status(200);
	let id = uuid_v4().substr(0,6);

	//if (debugFilter.get('login'))
	console.log((new Date()).toLocaleString() + ": login  <-" + id);
	
	user.push(id);
	res.end(id);
});
app.get('/getData', function (req, res) {
	let sender = req.headers['x-id'];

	if (debugFilter.get('getData'))
		console.log((new Date()).toLocaleString() + ": getData <-" + req.headers['x-id']);
	
	for (let i = 0; i < user.length; ++i)
	{
		if (user[i] != sender)
		{
			res.json(user_data[user[i]]);
			res.status(200);
			return;
		}
	}

	
	res.status(404);
	res.end();
});
app.post('/addStack', function (req, res) {
	let sender = req.headers['x-id'];
	
	if (!user.includes(req.headers['x-id']))
	{	
		user.push(req.headers['x-id']);
		console.log((new Date()).toLocaleString() + ": AddUser<-" + req.headers['x-id']);
	}

	if (user_stack.get(sender) == undefined)
	{
		user_stack.set(sender, 1);
	}
	else
	{
		user_stack.set(sender, user_stack.get(sender) + 1);
	}
	if (debugFilter.get('addStack'))
	{
		console.log((new Date()).toLocaleString() + ": addStack-" + req.headers['x-id'] + ': ' + sender + " up to " + user_stack.get(sender));
	}
	
	res.status(200);
	res.end();
});

app.get('/getStack', function (req, res) {
	let sender = req.headers['x-id'];
	if (debugFilter.get('getStack'))
		console.log((new Date()).toLocaleString() + ": getStack-" + req.headers['x-id']);
	
	let stacks = [];
	for (let i = 0; i < user.length; ++i)
	{
		if (user[i] != sender)
		{
			if (user_stack.get(user[i]) == undefined) user_stack.set(user[i], 0);

			stacks.push({
				'uuid': user[i],
				'stack': user_stack.get(user[i])
			});
		}
	}
	
	res.json(stacks);
	res.status(200);
	res.end();
});

app.post('/sendData', function (req, res) {
	if (!user.includes(req.headers['x-id']))
	{	
		user.push(req.headers['x-id']);
		if (debugFilter.get('AddUser'))
			console.log((new Date()).toLocaleString() + ": AddUser<-" + req.headers['x-id']);
	}

	let data = req.body;

	data['time'] = (new Date()).toLocaleString();
	data['uuid'] = req.headers['x-id'];

	user_data[req.headers['x-id']] = data;
	if (debugFilter.get('sendData'))
		console.log((new Date()).toLocaleString() + ": sendData<-" + req.headers['x-id']);
	res.end();
});

var server = https.createServer(opts, app).listen(443, function () {
   var host = server.address().address;
   var port = server.address().port;
   console.log("Example app listening at https://%s:%s", host, port);
})

setInterval(()=>{
	if (user.length > 2)
	{
		let user2 = user;
		user = [];
		for (let i = 1; i < user2.length; ++i)
		{
			user.push(user2[i]);
		}
	}
}, 100);