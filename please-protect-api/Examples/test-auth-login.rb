#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = "temp"
keyFile = ".token"

apiUrl = "api/Auth/org/#{orgId}/action/Login"
param =  {
  UserName: "#{ENV['USER_NAME']}",
  Password: "#{ENV['USER_PASSWORD']}",
}

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

result = make_request(:post, apiUrl, param)
#puts(result)

#token = result["token"]["access_token"]
#puts(token)

apiUrl = "api/Auth/org/#{orgId}/action/Refresh"
param =  {
  RefreshToken: result["token"]["refresh_token"],
}
result = make_request(:post, apiUrl, param)

token = result["token"]["access_token"]
puts(token)

File.write(keyFile, token)

