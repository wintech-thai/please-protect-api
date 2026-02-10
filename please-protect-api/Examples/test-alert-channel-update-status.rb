#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = '15a271f5-b52a-4c88-84ac-ff5a1b911dc4'

apiUrl = "api/AlertChannel/org/#{orgId}/action/EnableAlertChannelById/#{id}"
param = nil

result = make_request(:post, apiUrl, param)
json = result.to_json
puts(json)
