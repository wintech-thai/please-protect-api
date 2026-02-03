#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = 'd8417099-8738-4004-8223-e61301a8effa'

apiUrl = "api/CustomRole/org/#{orgId}/action/DeleteCustomRoleById/#{id}"
param = nil

result = make_request(:delete, apiUrl, param)

json = result.to_json
puts(json)
