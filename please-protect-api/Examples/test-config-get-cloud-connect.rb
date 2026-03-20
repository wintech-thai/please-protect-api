#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
apiName1 = "SetCloudUrl" #SetCloudConnectKey, SetCloudUrl, SetCloudConnectFlag
apiName2 = "GetCloudUrl" #GetCloudConnectKey, GetCloudUrl, GetCloudConnectFlag
configValue = "https://api-dev.please-protect.com/api/Agent/org/rtarf/action/Heartbeat/this-is-fake-agent-id" #"https://api-dev.please-protect.com/api/Agent/org/rtarf/action/Heartbeat/this-is-fake-agent-id"

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
