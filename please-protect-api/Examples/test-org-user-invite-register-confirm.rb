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

url = "https://abc.com/user-signup-confirm/e69498ff-5cd5-4c43-a01f-a06c71a8e33b?data=eyJFbWFpbCI6InBqYW1lLmZiMkBnbWFpbC5jb20iLCJVc2VyTmFtZSI6InNldWJwb25nLm1vbjIiLCJQYXNzd29yZCI6bnVsbCwiTmFtZSI6bnVsbCwiTGFzdG5hbWUiOm51bGwsIkludml0ZWRCeSI6ImFwaSIsIk9yZ1VzZXJJZCI6IjZiMjkzNDcwLTQxMWEtNGZmYS1hNDA0LTBmODk5ZGZkMDA5MCJ9"
uri = URI.parse(url)

# แปลง query string เป็น hash
params = CGI.parse(uri.query)
data = params['data'].first

path = uri.path
parts = path.split('/').reject(&:empty?)

decoded = Base64.decode64(data)
dataObj = JSON.parse(decoded)

orgId = 'default'
regType = parts[0]
token = parts[1]

api = "ConfirmNewUserInvitation"
if (regType != 'user-signup-confirm')
  api = "ConfirmExistingUserInvitation"
end

#puts(dataObj)
userName = dataObj['UserName']

param =  {
  Email: dataObj['Email'],
  UserName: "#{userName}",
  Password: "#{ENV['USER_PASSWORD']}",
  Name: "Seubpong",
  LastName: "Monsar",
  OrgUserId: dataObj['OrgUserId'],
}

#puts(param)
#puts(parts)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

apiUrl = "api/Registration/org/#{orgId}/action/#{api}/#{token}/#{userName}"
#puts(apiUrl)

result = make_request(:post, apiUrl, param)
puts(result)
