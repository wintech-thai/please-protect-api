#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = 'eb8ad1c0-113b-4cd9-905d-8781defd8f9f'

apiUrl = "api/CustomRole/org/#{orgId}/action/GetCustomRoleById/#{id}"
param = nil

result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
