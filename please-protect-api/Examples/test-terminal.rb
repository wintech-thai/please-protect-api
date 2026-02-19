#!/usr/bin/env ruby

require 'websocket-client-simple'
require 'base64'
require './utils'

$stdout.sync = true
$stdin.sync  = true

load_env("./.env")

orgId = ENV['API_ORG']
apiUrl = "wss://api-dev.rtarf-censor.dev-hubs.com/api/Terminal/org/#{orgId}/action/Connect"

username = 'api'
password = ENV['API_KEY']

credentials = Base64.strict_encode64("#{username}:#{password}")

ws = WebSocket::Client::Simple.connect(
  apiUrl,
  headers: {
    "Authorization" => "Basic #{credentials}"
  }
)

ws.on :open do
  puts "✅ Connected to terminal"
  print "> "
end

ws.on :message do |msg|
  print "\r"           # reset cursor
  print msg.data       # แสดง output จาก pod
  print "\n> "         # แสดง prompt ใหม่
end

ws.on :error do |e|
  puts "\n❌ Error: #{e.message}"
end

ws.on :close do
  puts "\n🔒 Connection closed"
  exit
end

# Thread สำหรับอ่าน input จากคุณ
Thread.new do
  while line = STDIN.gets
    ws.send(line)   # ส่งสิ่งที่คุณพิมพ์ไปที่ pod
  end
end

# ป้องกัน script จบ
loop { sleep 1 }
