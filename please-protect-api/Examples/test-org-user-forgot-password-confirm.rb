#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require 'cgi'
require 'base64'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

url = "https://abc.com/forgot-password/default/888e7afc-bb1f-4ab9-ba5e-af2b9fd58805?data=eyJFbWFpbCI6InBqYW1lLmZiQGdtYWlsLmNvbSIsIlVzZXJOYW1lIjoic2V1YnBvbmcubW9uIiwiUGFzc3dvcmQiOm51bGwsIk5hbWUiOm51bGwsIkxhc3RuYW1lIjpudWxsLCJJbnZpdGVkQnkiOm51bGwsIk9yZ1VzZXJJZCI6IjA2N2JkZWYyLTA0NzktNDEwOC1iZmJjLWQ4NDRhMDhiMjk3ZSJ9"
uri = URI.parse(url)

# แปลง query string เป็น hash
params = CGI.parse(uri.query)
data = params['data'].first

path = uri.path
parts = path.split('/').reject(&:empty?)

decoded = Base64.decode64(data)
dataObj = JSON.parse(decoded)

regType = parts[0]
orgId = parts[1]
token = parts[2]

api = "ConfirmForgotPasswordReset"

#puts(dataObj)
userName = dataObj['UserName']

param =  {
  Email: dataObj['Email'],
  UserName: "#{userName}",
  Password: "#{ENV['USER_PASSWORD']}",
  OrgUserId: dataObj['OrgUserId'],
}

#puts(param)
#puts(parts)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

apiUrl = "api/Registration/org/#{orgId}/action/#{api}/#{token}/#{userName}"
#puts(apiUrl)
puts("DEBUG Email=[#{dataObj['Email']}], Username=[#{userName}], OrgUserId=[#{dataObj['OrgUserId']}]")
result = make_request(:post, apiUrl, param)
puts(result)
