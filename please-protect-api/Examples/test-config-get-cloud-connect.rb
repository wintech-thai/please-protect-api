#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
apiName1 = "SetCloudConnectFlag" #SetCloudConnectKey, SetCloudUrl, SetCloudConnectFlag
apiName2 = "GetCloudConnectFlag" #GetCloudConnectKey, GetCloudUrl, GetCloudConnectFlag
configValue = "false" #"https://api-dev.please-protect.com/api/Agent/org/rtarf/action/Heartbeat"

apiUrl = "api/Configuration/org/#{orgId}/action/#{apiName1}"
param = {
  ConfigValue: configValue
}

result = make_request(:post, apiUrl, param)
json = result.to_json
puts(json)

apiUrl = "api/Configuration/org/#{orgId}/action/#{apiName2}"
param = nil

result = make_request(:get, apiUrl, param)
json = result.to_json
puts(json)
