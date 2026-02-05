#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/Subnet/org/#{orgId}/action/UpdateSubnetsCache"
param = nil

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
