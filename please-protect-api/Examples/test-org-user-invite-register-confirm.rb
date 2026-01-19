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

url = "https://register.otep.triple-t.co/default/user-signup-confirm/d70c4192-e053-4230-b751-749c0bc38347?data=eyJFbWFpbCI6InBqYW1lLmZiQGdtYWlsLmNvbSIsIlVzZXJOYW1lIjoic2V1YnBvbmcubW9uIiwiUGFzc3dvcmQiOm51bGwsIk5hbWUiOm51bGwsIkxhc3RuYW1lIjpudWxsLCJJbnZpdGVkQnkiOiJhcGkiLCJPcmdVc2VySWQiOiI5NDFjNzdlOC0yMTNhLTQ1N2UtOTg5Yi03NTI1Zjc2MzU4NTUifQ%3d%3d"
uri = URI.parse(url)

# แปลง query string เป็น hash
params = CGI.parse(uri.query)
data = params['data'].first

path = uri.path
parts = path.split('/').reject(&:empty?)

decoded = Base64.decode64(data)
dataObj = JSON.parse(decoded)

orgId = parts[0]
regType = parts[1]
token = parts[2]

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
